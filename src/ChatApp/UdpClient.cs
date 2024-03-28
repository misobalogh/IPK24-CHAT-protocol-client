using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using ChatApp.Enums;
using Timer = System.Timers.Timer;
using ChatApp.Messages;

namespace ChatApp
{
    public class UdpClient : ClientBase
    {
        private readonly string _serverAddress;
        private readonly int _serverPort;
        private readonly System.Net.Sockets.UdpClient _udpClient;
        private readonly ushort _confirmationTimeout;
        private readonly byte _maxRetransmissions;
        private readonly Dictionary<ushort, StoredMessage> _storedMessages = new();

        public UdpClient(string serverAddress, ushort serverPort, ushort udpTimeout, byte maxRetransmissions)
        {
            _serverAddress = serverAddress;
            _serverPort = serverPort;
            _udpClient = new System.Net.Sockets.UdpClient();
            _udpClient.Client.ReceiveTimeout = udpTimeout;
            _udpClient.Client.Bind(new IPEndPoint(IPAddress.Any, 0));

            _confirmationTimeout = udpTimeout;
            _maxRetransmissions = maxRetransmissions;
        }

        public override async Task SendMessageAsync(Message message)
        {
            try
            {
                var byteMessage = message.CraftUdp();
                if (byteMessage != null)
                {
                    await _udpClient.SendAsync(byteMessage, byteMessage.Length, _serverAddress, _serverPort);
                    if (message.Type != MessageType.Confirm)
                    {
                        StartConfirmationTimer(message.MessageId, byteMessage);
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.InternalError($"Error sending message: {ex.Message}");
                throw;
            }
        }

        public override async Task<Message?> ReceiveMessageAsync()
        {
            try
            {
                UdpReceiveResult result = await _udpClient.ReceiveAsync();
                Message? message = MessageParser.ParseMessage(result.Buffer);
                if (message == null)
                {
                    return null;
                }

                if (message is { Type: MessageType.Confirm })
                {
                    StopConfirmationTimer(message.MessageId);
                }
                else
                {
                    SendConfirm(message.MessageId);
                    return message;
                }

                return null;
            }
            catch (Exception ex)
            {
                ErrorHandler.InternalError($"Error receiving message: {ex.Message}");
                throw;
            }
        }

        public override void Close()
        {
            _udpClient.Close();

            foreach (var storedMessage in _storedMessages.Values)
            {
                storedMessage.Timer?.Dispose();
            }
            _storedMessages.Clear();
        }

        private void StartConfirmationTimer(ushort messageId, byte[] byteMessage)
        {
            var timer = new Timer(_confirmationTimeout);
            timer.Elapsed += (sender, e) => OnConfirmationTimeout(messageId);
            timer.AutoReset = false;
            timer.Start();
            if (!_storedMessages.ContainsKey(messageId))
            {
                _storedMessages[messageId] = new StoredMessage(0, byteMessage, timer);
            }
        }

        private void StopConfirmationTimer(ushort messageId)
        {
            if (_storedMessages.TryGetValue(messageId, out var storedMessage))
            {
                Console.WriteLine($"removed message {messageId}");
                storedMessage.Timer.Stop();
                storedMessage.Timer.Dispose();
                _storedMessages.Remove(messageId);
            }
        }

        private async void OnConfirmationTimeout(ushort messageId)
        {
            foreach (var kvp in _storedMessages)
            {
                Console.WriteLine($"Message ID: {kvp.Key}, Retransmission Count: {kvp.Value.RetransmissionCount}");
            }

            if (_storedMessages.TryGetValue(messageId, out var storedMessage))
            {
                Console.WriteLine("Trying to send again");
                var retries = storedMessage.RetransmissionCount;
                if (retries < _maxRetransmissions)
                {
                    try
                    {
                        await _udpClient.SendAsync(storedMessage.ByteMessage, storedMessage.ByteMessage.Length, _serverAddress, _serverPort);
                        storedMessage.RetransmissionCount++;
                        
                        storedMessage.Timer.Stop();
                        storedMessage.Timer.Start();
                    }
                    catch (Exception ex)
                    {
                        ErrorHandler.InternalError($"Error retransmitting message: {ex.Message}");
                    }
                }
                else
                {
                    ErrorHandler.ExitWith($"Max retransmission count reached. Failed to send message", ExitCode.ConnectionError);
                }
            }
        }

        private async void SendConfirm(ushort refId)
        {
            Message message = new ConfirmMessage(refId);
            try
            {
                await SendMessageAsync(message);
            }
            catch (Exception ex)
            {
                ErrorHandler.InternalError($"Error while sending confirm message: {ex.Message}");
            }
        }

        private class StoredMessage(byte retransmissionCount, byte[] byteMessage, Timer timer)
        {
            public byte RetransmissionCount { get; set; } = retransmissionCount;
            public byte[] ByteMessage { get; } = byteMessage;
            public Timer Timer { get; set; } = timer;
        }
    }
}
