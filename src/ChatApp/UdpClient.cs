using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using ChatApp.Messages;

namespace ChatApp;

public class UdpClient : ClientBase
{
    private readonly string _serverAddress;
    private readonly int _serverPort;
    private readonly System.Net.Sockets.UdpClient _udpClient;

    public UdpClient(string serverAddress, ushort serverPort, ushort udpTimeout, byte maxRetransmissions)
    {
        _serverAddress = serverAddress;
        _serverPort = serverPort;
        _udpClient = new System.Net.Sockets.UdpClient();
        _udpClient.Client.ReceiveTimeout = udpTimeout;
        _udpClient.Client.Bind(new IPEndPoint(IPAddress.Any, 0));
    }

    public override async Task SendMessageAsync(object? message)
    {
        if (message is byte[] byteMessage)
        {
            try
            {
                await _udpClient.SendAsync(byteMessage, byteMessage.Length, _serverAddress, _serverPort);
            }
            catch (Exception ex)
            {
                ErrorHandler.InternalError($"Error sending message: {ex.Message}");
                throw;
            }
        }
        else
        {
            ErrorHandler.ExitWith("Invalid type of message to send", ExitCode.UnknownParam);
        }
    }

    public override async Task<object?> ReceiveMessageAsync()
    {
        try
        {
            UdpReceiveResult result = await _udpClient.ReceiveAsync();
            var port = result.RemoteEndPoint.Port;
            
            return result.Buffer;
        }
        catch (Exception ex)
        {
            ErrorHandler.InternalError($"Error receiving message: {ex.Message}");
            throw;
        }
    }

    public void Close()
    {
        _udpClient.Close();
    }
}
