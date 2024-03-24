using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ChatApp;

public class UdpClient : ClientBase
{
    private int _maxMessageSize = 2048;
    private readonly byte _maxRetransmissions;
    private readonly IPEndPoint _endPoint = null!;

    public UdpClient(string serverAddress, ushort serverPort, ushort udpTimeout, byte maxRetransmissions)
    {
        _maxRetransmissions = maxRetransmissions;
            
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
                
            _endPoint = new IPEndPoint(address, serverPort);
                
            Socket = new Socket(address.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
            Socket.ReceiveTimeout = udpTimeout;
        }
        catch(Exception ex)
        {
            Close();
            ErrorHandler.ExitWith($"Error occurred when connecting to the server: {ex.Message}", ExitCode.ConnectionError);
        }
    }

    public override async Task SendMessageAsync(object? message)
    {
        if (message is byte[] byteArrayMessage)
        {
            for (byte retransmissionCount = 0; retransmissionCount < _maxRetransmissions; retransmissionCount++)
            {
                try
                {
                    await Socket.SendToAsync(byteArrayMessage, SocketFlags.None, _endPoint);
                    return;
                }
                catch (SocketException)
                {
                    // transmission error
                }
            }

            // max retransmission failure
        }
        else
        {
            // invalid message type
        }
    }

    public override async Task<object?> ReceiveMessageAsync()
    {
        try
        {
            var recBuffer = new byte[_maxMessageSize]; 
            var numOfReceivedBytes = await Socket.ReceiveFromAsync(recBuffer, SocketFlags.None, _endPoint);
            return recBuffer;
        }
        catch (SocketException)
        {
            return null;
        }
    }
}