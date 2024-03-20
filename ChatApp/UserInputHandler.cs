using System;
using System.Threading.Tasks;
using ChatApp.Enums;
using ChatApp.Messages;

namespace ChatApp
{
    public class UserInputHandler(string transportProtocol, string serverAddress, ushort serverPort, ushort udpTimeout, byte maxRetransmissions)
    {
        private bool _exit;
        private string _displayName = "";
        private readonly TcpClient _tcpClient = new(serverAddress, serverPort);
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

                if (string.IsNullOrWhiteSpace(input))
                {
                    continue;
                }

                if (input.StartsWith('/'))
                {
                    ProcessLocalCommand(input);
                }
                else
                {
                   Message message = new MsgMessage(_displayName, input);
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
                _messageQueue.Enqueue(message);
                
                if (_waitingForReply)
                {
                    return;
                }

                var messageToProcess = _messageQueue.Dequeue();
                if (messageToProcess.Type is MessageType.Auth or MessageType.Join)
                {
                    _waitingForReply = true;
                }
                
                bool isValidMessageType = messageToProcess.Type == _possibleClientMessageType ||
                                          (_possibleClientMessageType == MessageType.MsgOrJoin &&
                                           messageToProcess.Type is MessageType.Msg or MessageType.Join);
                
                if (!isValidMessageType && !_waitingForReply)
                {
                    ErrorHandler.InternalError($"Cannot send message of type {messageToProcess.Type} in the current client state");
                    return;
                }
                
                await SendMessageAsync(messageToProcess.Craft());
            }
            finally
            {
                _messageSemaphore.Release();
            }
        }

        private async Task SendMessageAsync(string? messageContent)
        {
            if (messageContent != null)
            {
                await _tcpClient.SendMessageAsync(messageContent);
            }
        }

        private async Task ReceiveMessagesAsync()
        {
            try
            {
                while (!_exit)
                {
                    string? reply = await _tcpClient.ReceiveMessageAsync();
                    if (reply == null)
                    {
                        continue;
                    }
                    
                    Message? message = MessageParser.ParseMessage(reply);
                    _receivedMessageType = message?.Type ?? MessageType.None;
                    
                    if (_receivedMessageType == MessageType.Bye)
                    {
                        _tcpClient.Close();
                        ErrorHandler.InformUser("Connection terminated from server");
                        ErrorHandler.ExitSuccess();
                    }

                    if (_receivedMessageType == MessageType.Err)
                    {
                        await _tcpClient.SendMessageAsync(new ByeMessage().Craft());
                        _tcpClient.Close();
                        ErrorHandler.ExitSuccess();
                    }
                    
                    if (_receivedMessageType == MessageType.Reply)
                    {
                        _waitingForReply = false;
                    }

                    _clientState.NextState(_receivedMessageType, out _possibleClientMessageType);

                    if (_clientState.GetCurrentState() == State.Error)
                    {
                        await _tcpClient.SendMessageAsync(new ErrMessage(_displayName,"Error occured while receiving message from server").Craft());
                        message?.PrintOutput();
                        _clientState.NextState(_receivedMessageType, out _possibleClientMessageType);
                        await _tcpClient.SendMessageAsync(new ByeMessage().Craft());
                        ErrorHandler.ExitSuccess();
                    }
                    
                    message?.PrintOutput();
                    
                    await _messageSemaphore.WaitAsync();

                    try
                    {
                        if (!_waitingForReply)
                        {
                            while (_messageQueue.Count > 0 && !_waitingForReply)
                            {
                                var messageToSent = _messageQueue.Dequeue();
                                await SendMessageAsync(messageToSent.Craft());
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
                ErrorHandler.InternalError($"Error while receiving messages: {ex.Message}");
            }
        }

        private async void OnCancelKeyPress(object? sender, ConsoleCancelEventArgs eventArgs)
        {
            if (eventArgs.SpecialKey == ConsoleSpecialKey.ControlC)
            {
                eventArgs.Cancel = true;
                Message message = new ByeMessage();
                try
                {
                    await _tcpClient.SendMessageAsync(message.Craft());
                    ErrorHandler.ExitSuccess();
                }
                catch (Exception ex)
                {
                    ErrorHandler.InternalError($"Error sending message on cancel: {ex.Message}");
                }
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

            Message message = new AuthMessage(username, _displayName, secret);
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
                ErrorHandler.InternalError("Cannot use /join in the current state of the client");
                return;
            }

            string channelId = parametersSplit[0];
            Message message = new JoinMessage(channelId, _displayName);
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
