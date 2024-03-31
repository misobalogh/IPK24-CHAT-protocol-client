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

/// <summary>
/// Client for handling communication for the TCP variant.
/// </summary>
public class TcpClient : ClientBase
{
    private bool _connectionTerminated;
    private readonly NetworkStream _stream = null!;
    private readonly StreamWriter _writer = null!;
    private readonly StreamReader _reader = null!;
    private readonly Socket _socket = null!;
    
    /// <summary>
    /// Creates a new instance of the <see cref="TcpClient"/> class with specified server address and port to connect to.
    /// <param name="serverAddress">The IP address or hostname of the server.</param>
    /// <param name="serverPort">The port number of the server.</param>
    /// </summary>
    public TcpClient(string serverAddress, int serverPort)
    {
        try
        {
            // try to resolve the server address
            if (!IPAddress.TryParse(serverAddress, out var address))
            {
                IPAddress[] addresses = Dns.GetHostAddresses(serverAddress);
                if (addresses.Length == 0)
                {
                    ErrorHandler.ExitWith("Failed to resolve server address", ExitCode.ConnectionError);
                }

                address = addresses[0];
            }

            _socket = new Socket(address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            IPEndPoint endPoint = new IPEndPoint(address, serverPort);
            
            _socket.Connect(endPoint);

            _stream = new NetworkStream(_socket);

            _writer = new StreamWriter(_stream, Encoding.ASCII);
            _reader = new StreamReader(_stream, Encoding.ASCII);
        }
        catch(Exception ex)
        {
            Close();
            ErrorHandler.ExitWith($"Error occurred when connecting to the server: {ex.Message}", ExitCode.ConnectionError);
        }
    }

    /// <summary>
    /// Sends asynchronously message to connected server
    /// </summary>
    /// <param name="message">Message to send</param>
    public override async Task SendMessageAsync(Message message)
    {
        try
        {
            if (!_connectionTerminated)
            {
                await _writer.WriteAsync(message.CraftTcp());
                await _writer.FlushAsync();
            }
        }
        catch (Exception ex)
        {
            ErrorHandler.InternalError($"Error sending message: {ex.Message}");
            Close();
            throw;
        }
    }

    /// <summary>
    /// Asynchronously receive message from the server.
    /// </summary>
    /// <returns>Received message or dummy InvalidMessage indicating error when parsing the received message or when the connection is terminated</returns>
    public override async Task<Message?> ReceiveMessageAsync()
    {
        try
        {
            if (!_connectionTerminated)
            {
                var message = await _reader.ReadLineAsync();
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
            Close();
            throw;
        }
    }
    
    /// <summary>
    /// Closes connection with server.
    /// </summary>
    public sealed override void Close()
    {
        _reader?.Close();
        _writer?.Close();
        _stream?.Close();
        _socket?.Close();
    }
}
