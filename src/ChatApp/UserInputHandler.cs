/*
 * File: UserInputHandler.cs 
 * Description: Class for handling user input, receiving responses and processing commands.
 * Author: Michal Balogh, xbalog06
 * Date: 30.03.2024
 */

using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using ChatApp.Enums;
using ChatApp.Messages;

namespace ChatApp
{
    public class UserInputHandler(ProtocolVariant transportProtocol, string serverAddress, ushort serverPort, ushort udpTimeout, byte maxRetransmissions)
    {
        private bool _exit;
        private string _displayName = "";
        private ushort _messageId = 0;

        private readonly ClientBase _client = transportProtocol == ProtocolVariant.Tcp
            ? new TcpClient(serverAddress, serverPort)
            : new UdpClient(serverAddress, serverPort, udpTimeout, maxRetransmissions);

        private readonly SemaphoreSlim _messageSemaphore = new(1, 1);
        private readonly Queue<Message> _messageQueue = new();
        private readonly ClientState _clientState = new();
        private bool _waitingForReply = false;
        private MessageType _receivedMessageType = MessageType.None;
        private MessageType _possibleClientMessageType = MessageType.Auth;
        

        public async Task ProcessInput()
        {
            _clientState.NextState(_receivedMessageType,out _possibleClientMessageType);
            
            _exit = false;
            Console.CancelKeyPress += OnCancelKeyPress;

            Task receivingTask = ReceiveMessagesAsync();

            while (!_exit)
            {
                string? input = Console.ReadLine();

                if (input == null)
                {
                    SendBye();
                }
                else if (string.IsNullOrWhiteSpace(input))
                {
                    continue;
                }
                else if (input.StartsWith('/'))
                {
                    ProcessLocalCommand(input);
                }
                else
                {
                   Message message = new MsgMessage(_displayName, input, _messageId++);
                   await EnqueueMessageAsync(message);
                }
            }
    
            await receivingTask;

            ErrorHandler.ExitSuccess();
        }

       private async Task EnqueueMessageAsync(Message message)  
        {
            await _messageSemaphore.WaitAsync();

            try
            {
                if (message.Type is MessageType.Join or MessageType.Auth && _waitingForReply)
                {
                    ErrorHandler.InternalError($"Wait for previous {message.Type} action to be processed");
                    return;
                }
                
                _messageQueue.Enqueue(message);
                
                if (_waitingForReply)
                {
                    return;
                }

                var messageToProcess = _messageQueue.Dequeue();
                
                bool isValidMessageType = messageToProcess.Type == _possibleClientMessageType ||
                                          (_possibleClientMessageType == MessageType.MsgOrJoin &&
                                           messageToProcess.Type is MessageType.Msg or MessageType.Join);
                if (!isValidMessageType)
                {
                    ErrorHandler.InternalError($"Cannot send message of type {messageToProcess.Type} in the current client state");
                    return;
                }
                
                if (messageToProcess.Type is MessageType.Auth or MessageType.Join)
                {
                    _waitingForReply = true;
                }
                
                await _client.SendMessageAsync(messageToProcess);
            }
            finally
            {
                _messageSemaphore.Release();
            }
        }

       private async Task ReceiveMessagesAsync()
        {
            try
            {
                while (!_exit)
                {
                    Message? message = await _client.ReceiveMessageAsync();
                    if (message == null)
                    {
                        continue;
                    }

                        
                    _receivedMessageType = message.Type;
                    
                    // ignore unexpected reply message
                    if (_receivedMessageType is MessageType.Reply or MessageType.NotReply)
                    {
                        if (!_waitingForReply)
                        {
                            continue;
                        }
                        _waitingForReply = false;
                    }
                    
                    if (_receivedMessageType == MessageType.Bye)
                    {
                        _client.Close();
                        ErrorHandler.ExitSuccess();
                    }

                    if (_receivedMessageType == MessageType.Err)
                    {
                        message?.PrintOutput();
                        SendBye();
                    }

                    if (_receivedMessageType != MessageType.Confirm)
                    {
                        _clientState.NextState(_receivedMessageType, out _possibleClientMessageType);
                    }
                    
                    if (_clientState.GetCurrentState() == State.Error)
                    {
                        await _client.SendMessageAsync(new ErrMessage(_displayName,"Error occured while receiving message from server", _messageId++));
                        _clientState.NextState(_receivedMessageType, out _possibleClientMessageType);
                    }

                    if (_clientState.GetCurrentState() == State.End)
                    {
                        await _client.SendMessageAsync(new ByeMessage(_messageId++));
                    }
                    
                    message?.PrintOutput();
                    
                    await _messageSemaphore.WaitAsync();

                    try
                    {
                        if (!_waitingForReply)
                        {
                            // send all messages that were stopped until reply for join or auth were received
                            while (_messageQueue.Count > 0 && !_waitingForReply)
                            {
                                var messageToSent = _messageQueue.Dequeue();
                                await _client.SendMessageAsync(messageToSent);

                                // if message that need reply is in the queue, break the cycle and wait for reply from server
                                if (messageToSent.Type is MessageType.Auth or MessageType.Join)
                                {
                                    _waitingForReply = true;
                                }
                            }
                        }
                    }
                    finally
                    {
                        _messageSemaphore.Release();
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.ExitWith($"Error while receiving messages: {ex.Message}", ExitCode.ConnectionError);
            }
        }

        private void OnCancelKeyPress(object? sender, ConsoleCancelEventArgs eventArgs)
        {
            if (eventArgs.SpecialKey is ConsoleSpecialKey.ControlC)
            {
                eventArgs.Cancel = true;
                SendBye();
                
            }
        }
        
        private async void SendBye()
        {
            Message message = new ByeMessage(_messageId++);
            try
            {
                await _client.SendMessageAsync(message);
                ErrorHandler.ExitSuccess();
            }
            catch (Exception ex)
            {
                ErrorHandler.InternalError($"Error sending message bye: {ex.Message}");
            }
        }

        private void ProcessLocalCommand(string command)
        {
            string[] splitCommand = command.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);

            if (splitCommand.Length == 0)
            {
                ErrorHandler.ExitWith("Empty command", ExitCode.UnknownCommand);
            }

            string localCommand = splitCommand[0].ToLower();
            string parameters = splitCommand.Length > 1 ? splitCommand[1] : "";

            switch (localCommand)
            {
                case "/auth":
                    Task.Run(async () => await HandleCommandAuth(parameters));
                    break;
                case "/join":
                    Task.Run(async () => await HandleCommandJoin(parameters));
                    break;
                case "/rename":
                    HandleCommandRename(parameters);
                    break;
                case "/help":
                    HandleCommandHelp(parameters);
                    break;
                default:
                    ErrorHandler.InternalError($"Unknown command: {localCommand}. Try /help for list of supported commands.");
                    return;
            }
        }

        private async Task HandleCommandAuth(string parameters)
        {
            string[] parametersSplit = parameters.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            if (parametersSplit.Length != 3)
            {
                ErrorHandler.InternalError("Wrong parameters for command /auth. Try /help");
                return;
            }
            
            string username = parametersSplit[0];
            string secret = parametersSplit[1];
            _displayName = parametersSplit[2];

            Message message = new AuthMessage(username, _displayName, secret, _messageId++);
            await EnqueueMessageAsync(message);
        }

        private async Task HandleCommandJoin(string parameters)
        {
            string[] parametersSplit = parameters.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            
            if (parametersSplit.Length != 1)
            {
                ErrorHandler.InternalError("Wrong parameters for command /join. Try /help");
                return;
            }

            if (_clientState.GetCurrentState() != State.Open)
            {
                ErrorHandler.InternalError($"Cannot use /join in the current state of the client. State {_clientState.GetCurrentState()}");
                return;
            }

            string channelId = parametersSplit[0];
            Message message = new JoinMessage(channelId, _displayName, _messageId++);
            await EnqueueMessageAsync(message);
        }

        private void HandleCommandRename(string parameters)
        {
            string[] parametersSplit = parameters.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            if (parametersSplit.Length != 1)
            {
                ErrorHandler.InternalError("Wrong parameters for command /rename. Try /help");
                return;
            }

            _displayName = parametersSplit[0];
        }

        private void HandleCommandHelp(string parameters)
        {
            if (!string.IsNullOrEmpty(parameters))
            {
                ErrorHandler.InternalError($"Wrong parameters - command /help does not support any parameters");
                return;
            }
            Console.WriteLine("Available commands:");
            Console.WriteLine("\t/auth {Username} {Secret} {DisplayName} Sends AUTH message with the data provided from the command to the server");
            Console.WriteLine("\t/join {ChannelID}\t\t\tSends JOIN message with channel name from the command to the server");
            Console.WriteLine("\t/rename {DisplayName}\t\t\tchanges the display name of the user ");
            Console.WriteLine("\t/help\t\t\t\t\tprints this help message");
        }
    }
}
