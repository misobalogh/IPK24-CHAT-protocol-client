/*
 * File: UdpClient.cs
 * Description: Implementation of the UDP variant of the client.
 * Author: Michal Balogh, xbalog06
 * Date: 30.03.2024
 */


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

namespace ChatApp;

public class UdpClient : ClientBase
{
    private int _serverPort;
    private readonly System.Net.Sockets.UdpClient _udpClient = new();
    private readonly Queue<Message> _messageQueue = new();
    private bool _boundPort = false;
    private readonly string _serverAddress;
    private readonly byte _maxRetransmissions;
    private readonly Timer _confirmationTimer;
    private Message _currentlyProcessedMessage = null!;
    private byte _retransmissionCount = 0;
    public UdpClient(string serverAddress, ushort serverPort, ushort udpTimeout, byte maxRetransmissions)
    {
        _serverAddress = serverAddress;
        _maxRetransmissions = maxRetransmissions;
        _serverPort = serverPort;
        _udpClient.Client.Bind(new IPEndPoint(IPAddress.Any, 0));
            
        _confirmationTimer = new Timer(udpTimeout);
        _confirmationTimer.AutoReset = false;
        _confirmationTimer.Elapsed += OnConfirmTimeout;
    }

    public override async Task SendMessageAsync(Message? message)
    {
        try
        {
            if (message == null)
            {
                return;
            }
                
            if (_confirmationTimer.Enabled)
            {
                _messageQueue.Enqueue(message);
            }
            else
            {
                _currentlyProcessedMessage = message;
                await SendMessageInternalAsync();
            }
        }
        catch (Exception ex)
        {
            ErrorHandler.InternalError($"Error sending message: {ex.Message}");
            Close();
            throw;
        }
    }
        
    private async Task SendMessageInternalAsync()
    {
        if (_currentlyProcessedMessage.CraftUdp() is { } byteMessage)
        {
            await _udpClient.SendAsync(byteMessage, byteMessage.Length, _serverAddress, _serverPort);
            if (_currentlyProcessedMessage.Type != MessageType.Confirm)
            {
                StartConfirmTimer();
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
                return new InvalidMessage();
            }

            if (message is { Type: MessageType.Confirm })
            {
                if (_currentlyProcessedMessage.MessageId != message.MessageId)
                {
                    return null;
                }
                    
                OnConfirmReceived(message.MessageId);
                    
                if (_messageQueue.Count <= 0)
                {
                    return null;
                }
                    
                var queuedMessage = _messageQueue.Dequeue();
                await SendMessageAsync(queuedMessage);
            }
            else
            {
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
        catch (Exception)
        {
            Close();
            throw;
        }
    }

    public override void Close()
    {
        _udpClient.Close();
        _messageQueue.Clear();
    }

    private void StartConfirmTimer()
    {
        _confirmationTimer.Stop();
        _confirmationTimer.Start();
    }

    private void OnConfirmReceived(ushort messageId)
    {
        _confirmationTimer.Stop();
        _retransmissionCount = 0;
    }

    private async void OnConfirmTimeout(object? sender, ElapsedEventArgs elapsedEventArgs)
    {
        if (_retransmissionCount < _maxRetransmissions)
        {
            try
            {
                StartConfirmTimer();
                if (_currentlyProcessedMessage.CraftUdp() is { } byteMessage)
                {
                    await _udpClient.SendAsync(byteMessage, byteMessage.Length, _serverAddress, _serverPort);
                    _retransmissionCount++;
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
            Close();
            ErrorHandler.ExitWith($"Error while sending confirm message: {ex.Message}", ExitCode.ConnectionError);
        }
    }
}