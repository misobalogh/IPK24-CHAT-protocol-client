using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ChatApp;
public class TcpClient
{
    private readonly Socket _socket = null!;
    private readonly NetworkStream _stream = null!;
    private readonly StreamWriter _writer = null!;
    private readonly StreamReader _reader = null!;

    public TcpClient(string serverAddress, int serverPort)
    {
        try
        {
            IPAddress address = IPAddress.Parse(serverAddress);
            
            _socket = new Socket(address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            
            IPEndPoint endPoint = new IPEndPoint(address, serverPort);
            _socket.Connect(endPoint);
            
            _stream = new NetworkStream(_socket);
            
            _writer = new StreamWriter(_stream, Encoding.ASCII);
            _reader = new StreamReader(_stream, Encoding.ASCII);
        }
        catch (Exception ex)
        {
            Close();
            ErrorHandler.ExitWith($"Error occured when connecting to the server: {ex.Message}", ExitCode.ConnectionError);
        }
    }

    public async Task SendMessageAsync(string? message)
    {
        
        try
        {
            await _writer.WriteLineAsync(message);
            await _writer.FlushAsync();
        }
        catch (Exception ex)
        {
            ErrorHandler.InternalError($"Error sending message: {ex.Message}");
            Close();
            throw;
        }
      
    }

    public async Task<string?> ReceiveMessageAsync()
    {
        try
        {
            return await _reader.ReadLineAsync();
        }
        catch (Exception ex)
        {
            ErrorHandler.InternalError($"Error receiving message: {ex.Message}");
            Close();
            throw;
        }
    }

    public void Close()
    {
        _reader.Close();
        _writer.Close();
        _stream.Close();
        _socket.Close();
    }
}

