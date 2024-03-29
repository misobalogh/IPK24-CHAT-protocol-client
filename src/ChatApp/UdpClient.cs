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
        private int _serverPort;
        private readonly System.Net.Sockets.UdpClient _udpClient = new();
        private readonly Queue<StoredMessage> _messageQueue = new();
        private bool _boundPort = false;
        private readonly string _serverAddress;
        private readonly ushort _udpTimeout;
        private readonly byte _maxRetransmissions;
        private readonly Timer _confirmationTimer;
        public UdpClient(string serverAddress, ushort serverPort, ushort udpTimeout, byte maxRetransmissions)
        {
            _serverAddress = serverAddress;
            _udpTimeout = udpTimeout;
            _maxRetransmissions = maxRetransmissions;
            _serverPort = serverPort;
            _udpClient.Client.Bind(new IPEndPoint(IPAddress.Any, 0));
            
            _confirmationTimer = new Timer(_udpTimeout);
            _confirmationTimer.AutoReset = false;
            _confirmationTimer.Elapsed += OnConfirmationTimeout;
        }

        public override async Task SendMessageAsync(Message message)
        {
            try
            {
                var byteMessage = message.CraftUdp();
                if (byteMessage == null)
                {
                    return;
                }

                if (_confirmationTimer.Enabled)
                {
                    _messageQueue.Enqueue(new StoredMessage(message));
                }
                else
                {
                    _messageQueue.Enqueue(new StoredMessage(message));
                    await SendMessageInternalAsync(message);
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.InternalError($"Error sending message: {ex.Message}");
                Close();
                throw;
            }
        }
        
        private async Task SendMessageInternalAsync(Message message)
        {
            if (message.CraftUdp() is { } byteMessage)
            {
                await _udpClient.SendAsync(byteMessage, byteMessage.Length, _serverAddress, _serverPort);
                if (message.Type != MessageType.Confirm)
                {
                    StartConfirmationTimer();
                }
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
                    if (_messageQueue.Count <= 0 || _messageQueue.First().Message.MessageId != message.MessageId)
                    {
                        return null;
                    }
                    OnConfirmReceived(message.MessageId);
                    var queuedMessage = _messageQueue.Dequeue();
                    await SendMessageInternalAsync(queuedMessage.Message);
                }
                else
                {
                    // if (message is ReplyMessage replyMessage)
                    // {
                    //     Console.WriteLine($"Reply: {message.MessageId}, {message.CraftTcp()}");
                    //     replyMessage.PrintRefId();
                    //
                    // }
                    if (!_boundPort)
                    {
                        _serverPort  = (ushort)result.RemoteEndPoint.Port;
                        _boundPort = true;
                    }
                    
                    SendConfirm(message.MessageId);
                    return message;
                }

                return null;
            }
            catch (Exception ex)
            {
                ErrorHandler.InternalError($"Error receiving message: {ex.Message}");
                Close();
                throw;
            }
        }

        public override void Close()
        {
            _udpClient.Close();
            _messageQueue.Clear();
        }

        private void StartConfirmationTimer()
        {
            var timer = new Timer(_udpTimeout);
            timer.Elapsed += OnConfirmationTimeout;
            timer.AutoReset = false;
            timer.Start();
        }

        private void OnConfirmReceived(ushort messageId)
        {
            Console.WriteLine($"Confirm received, message {messageId}");
            _confirmationTimer.Stop();
            if (_messageQueue.Count > 0)
            {
               _messageQueue.Dequeue();
            }
        }

        private async void OnConfirmationTimeout(object? sender, ElapsedEventArgs elapsedEventArgs)
        {
            if (_messageQueue.Count <= 0)
            {
                return;
            }
            var messageToProcess = _messageQueue.First();
            var retries = messageToProcess.RetransmissionCount;
            if (retries < _maxRetransmissions)
            {
                try
                {
                    if (messageToProcess.Message.CraftUdp() is { } byteMessage)
                    {
                        Console.WriteLine("Trying to send again");
                        await _udpClient.SendAsync(byteMessage, byteMessage.Length, _serverAddress, _serverPort);
                        messageToProcess.RetransmissionCount++;
                    }
                }
                catch (Exception ex)
                {
                    Close();
                    ErrorHandler.ExitWith($"Failed to send message. {ex}", ExitCode.ConnectionError);   
                }
            }
            else
            {
                Close();
                ErrorHandler.ExitWith($"Max retransmission count reached. Failed to send message", ExitCode.ConnectionError);
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

        private class StoredMessage(Message message)
        {
            public byte RetransmissionCount { get; set; } = 0;
            public Message Message { get; } = message;
        }
    }
}
