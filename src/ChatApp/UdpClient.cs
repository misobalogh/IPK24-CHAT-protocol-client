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

/// <summary>
/// Client for handling communication for the UDP variant.
/// </summary>
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
    
    /// <summary>
    /// Creates a new instance of the <see cref="UdpClient"/> class with specified server address, port to connect to, timeout and number of attempts for retransmitting messages.
    /// <param name="serverAddress">IP address or hostname of the server.</param>
    /// <param name="serverPort">Port number of the server.</param>
    /// <param name="udpTimeout">Timeout in ms for one attempt of retransmitting message.</param>
    /// <param name="maxRetransmissions">Number of attempts for retransmitting message.</param>
    /// </summary>
    public UdpClient(string serverAddress, ushort serverPort, ushort udpTimeout, byte maxRetransmissions)
    {
        _serverAddress = serverAddress;
        _maxRetransmissions = maxRetransmissions;
        _serverPort = serverPort;
        
        // bind to any local endpoint
        _udpClient.Client.Bind(new IPEndPoint(IPAddress.Any, 0));
            
        _confirmationTimer = new Timer(udpTimeout);
        _confirmationTimer.AutoReset = false;
        // subscribe method to elapsed event of the timer
        _confirmationTimer.Elapsed += OnConfirmTimeout;
    }

    /// <summary>
    /// Sends asynchronously message to server.
    /// </summary>
    /// <param name="message">Message to send</param>
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
        
    /// <summary>
    /// Private method that, tries to send the currently processed message and starts the timer to keep track of retransmission timeout.
    /// </summary>
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
    
    
    /// <summary>
    /// Asynchronously receive message from the server and sends confirmation if the message is correctly received.
    /// </summary>
    /// <returns>
    /// Received message, dummy InvalidMessage indicating error when parsing the received message or null,
    /// when no action is needed to be performed by the app
    /// </returns>
    public override async Task<Message?> ReceiveMessageAsync()
    {
        try
        {
            UdpReceiveResult result = await _udpClient.ReceiveAsync();
            Message? message = MessageParser.ParseMessage(result.Buffer);
            if (message == null)
            {
                // error when parsing messages
                return new InvalidMessage();
            }

            if (message is { Type: MessageType.Confirm })
            {
                // ignore confirm messages that are not referring to most current message send
                if (_currentlyProcessedMessage.MessageId != message.MessageId)
                {
                    return null;
                }
                
                OnConfirmReceived();
                    
                // if no messages were waiting in queue to be send, no more actions are needed
                if (_messageQueue.Count <= 0)
                {
                    return null;
                }
                   
                // in case messages there were messages waiting for the confirm, send one
                var queuedMessage = _messageQueue.Dequeue();
                await SendMessageAsync(queuedMessage);
            }
            else
            {
                // after first message other then confirm from server is received, bind the port to the one that it was sent from 
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

    /// <summary>
    /// Dispose resources.
    /// </summary>
    public override void Close()
    {
        _udpClient.Close();
        _messageQueue.Clear();
    }

    /// <summary>
    /// Reset timer for retransmission timeout.
    /// </summary>
    private void StartConfirmTimer()
    {
        _confirmationTimer.Stop();
        _confirmationTimer.Start();
    }

    /// <summary>
    /// Confirm message for most recent message was received.
    /// </summary>
    private void OnConfirmReceived()
    {
        _confirmationTimer.Stop();
        _retransmissionCount = 0;
    }

    /// <summary>
    /// This method is invoked, when the timer runs out. It will try to send the message again, while incrementing the retransmission count.
    /// If the retransmission count exceeds maximum amount of attempts, exits with error.
    /// </summary>
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

    
    /// <summary>
    /// Helper method, that sends confirm message.
    /// </summary>
    /// <param name="refId"></param>
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