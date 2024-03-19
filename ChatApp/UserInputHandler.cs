using System;
using System.Threading.Tasks;
using ChatApp.Enums;
using ChatApp.Messages;

namespace ChatApp
{
    public class UserInputHandler
    {
        private bool _exit;
        private string _displayName = "";
        private readonly TcpClient _tcpClient = new("127.0.0.1", 4567);
        private readonly SemaphoreSlim _messageSemaphore = new(1, 1);
        private readonly Queue<Message> _messageQueue = new();
        private bool _waitingForReply;
        private readonly ClientState _clientState = new();
        private MessageType _receivedMessageType = MessageType.None;
        private MessageType _sentMessageType = MessageType.None;

        public async Task ProcessInput()
        {
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
                    if (_clientState.GetCurrentState() == State.Open)
                    {
                        Message message = new MsgMessage(_displayName, input);
                        await EnqueueMessageAsync(message);
                    }
                    else
                    {
                        ErrorHandler.InternalError("Cannot send messages in current state. U have to use /auth first");
                    }
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

                
                    _sentMessageType = message.Type;
                    _clientState.NextState(_receivedMessageType, _sentMessageType);
                    
                    _waitingForReply = _clientState.GetCurrentState() == State.Auth;
                    
                    await SendMessageAsync(_messageQueue.Dequeue().Craft());
                    _sentMessageType = MessageType.None;
                    
                    Console.WriteLine($"STATE {_clientState.GetCurrentState()} {_receivedMessageType} {_sentMessageType}");
                    
                
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
                    message?.PrintOutput();

                    _clientState.NextState(_receivedMessageType, _sentMessageType);

                    _receivedMessageType = MessageType.None;
                    
                    Console.WriteLine($"STATE {_clientState.GetCurrentState()} {_receivedMessageType} {_sentMessageType}");

                    
                    // if (!_waitingForReply || _receivedMessageType != MessageType.Reply)
                    // {
                    //     continue;
                    // }
                    
                    await _messageSemaphore.WaitAsync();

                    try
                    {
                        if (_messageQueue.Count > 0)
                        {
                            await SendMessageAsync(_messageQueue.Dequeue().Craft());
                        }
                        else
                        {
                            _waitingForReply = false;
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

            if (_clientState.GetCurrentState() != State.Auth && _clientState.GetCurrentState() != State.Start)
            {
                _sentMessageType = MessageType.Auth;
                _clientState.NextState(_receivedMessageType, _sentMessageType);
                
                ErrorHandler.InternalError("Cannot use /auth in current state of the client");
                _sentMessageType = MessageType.None;
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
