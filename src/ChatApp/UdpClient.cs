using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ChatApp;

public class UdpClient(string serverAddress, ushort serverPort, ushort udpTimeout, byte maxRetransmissions) : ClientBase
{
    public override async Task SendMessageAsync(object? message)
    {
        throw new NotImplementedException();
    }

    public override async Task<string?> ReceiveMessageAsync()
    {
        throw new NotImplementedException();
    }
}
