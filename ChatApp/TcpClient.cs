using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using ChatApp.Messages;

namespace ChatApp
{
    public class TcpClient
    {
        private readonly Socket? _socket;
        private readonly NetworkStream? _stream;
        private readonly StreamWriter? _writer;
        private readonly StreamReader? _reader;

        public TcpClient(string serverAddress, int serverPort)
        {
            try
            {
                IPAddress address = IPAddress.Parse(serverAddress);
                
                _socket = new Socket(address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                
                IPEndPoint endPoint = new IPEndPoint(address, serverPort);
                _socket.Connect(endPoint);
                
                _stream = new NetworkStream(_socket);
                
                _writer = new StreamWriter(_stream);
                _reader = new StreamReader(_stream);
            }
            catch (Exception ex)
            {
                Close();
                ErrorHandler.ExitWith($"Error occured when connecting to the server: {ex.Message}", ExitCode.ConnectionError);
            }
        }

        public void SendMessage(string message)
        {
            try
            {
                _writer?.WriteLine(message);
                _writer?.Flush();
            }
            catch (Exception ex)
            {
                ErrorHandler.InformUser($"Error sending message: {ex.Message}");
                Close();
                throw;
            }
        }

        public string? ReceiveMessage()
        {
            try
            {
                return _reader?.ReadLine();
            }
            catch (Exception ex)
            {
                ErrorHandler.InformUser($"Error receiving message: {ex.Message}");
                Close();
                throw;
            }
        }

        public void Close()
        {
            _reader?.Close();
            _writer?.Close();
            _stream?.Close();
            _socket?.Close();
        }
    }
}
