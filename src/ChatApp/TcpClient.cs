/*
 * File: TcpClient.cs
 * Description: Implementation of the TCP variant of the client.
 * Author: Michal Balogh, xbalog06
 * Date: 30.03.2024
 */


using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using ChatApp.Enums;
using ChatApp.Messages;

namespace ChatApp;
public class TcpClient : ClientBase
{
    private bool _connectionTerminated;

    public TcpClient(string serverAddress, int serverPort)
    {
        try
        {
            if (!IPAddress.TryParse(serverAddress, out var address))
            {
                IPAddress[] addresses = Dns.GetHostAddresses(serverAddress);
                if (addresses.Length == 0)
                {
                    ErrorHandler.ExitWith("Failed to resolve server address", ExitCode.ConnectionError);
                }

                address = addresses[0];
            }

            Socket = new Socket(address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            IPEndPoint endPoint = new IPEndPoint(address, serverPort);
            Socket.Connect(endPoint);

            Stream = new NetworkStream(Socket);

            Writer = new StreamWriter(Stream, Encoding.ASCII);
            Reader = new StreamReader(Stream, Encoding.ASCII);
        }
        catch(Exception ex)
        {
            Close();
            ErrorHandler.ExitWith($"Error occurred when connecting to the server: {ex.Message}", ExitCode.ConnectionError);
        }
    }

    public override async Task SendMessageAsync(Message message)
    {
        try
        {
            if (!_connectionTerminated)
            {
                await Writer.WriteAsync(message.CraftTcp());
                await Writer.FlushAsync();
            }
        }
        catch (Exception ex)
        {
            ErrorHandler.InternalError($"Error sending message: {ex.Message}");
            Close();
            throw;
        }
    }

    public sealed override void Close()
    {
        Reader?.Close();
        Writer?.Close();
        Stream?.Close();
        Socket?.Close();
    }

    public override async Task<Message?> ReceiveMessageAsync()
    {
        try
        {
            if (!_connectionTerminated)
            {
                var message = await Reader.ReadLineAsync();
                if (message == null)
                {
                    _connectionTerminated = true;
                    ErrorHandler.ExitWith("Connection terminated by the server", ExitCode.ConnectionError);
                }
                else
                {
                    Message? parsedMessage = MessageParser.ParseMessage(message);
                    return parsedMessage ?? new InvalidMessage();
                }
            }
            return new InvalidMessage();
        }
        catch (Exception ex)
        {
            ErrorHandler.InternalError($"Error receiving message: {ex.Message}");
            Close();
            throw;
        }
    }
}
