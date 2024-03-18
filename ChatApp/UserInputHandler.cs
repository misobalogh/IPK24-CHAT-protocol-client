using System;
using System.Net.Sockets;
using ChatApp.Messages;

namespace  ChatApp;

public class UserInputHandler
{
    private bool _exit;
    private string _displayName = "";
    private readonly TcpClient _tcpClient = new("127.0.0.1", 4567);

    public void ProcessInput()
    {
        _exit = false;
        Console.CancelKeyPress += OnCancelKeyPress;
        
        while (!_exit)
        {
            string? input = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(input))
            {
                continue;
            }

            if (input.StartsWith("/"))
            {
                ProcessLocalCommand(input);
            }
            else
            {
                SendMessage(input);
            }

            string? reply = _tcpClient.ReceiveMessage();
            Console.WriteLine(reply);
        }
        
        ErrorHandler.ExitSuccess();
    }

    private void OnCancelKeyPress(object? sender, ConsoleCancelEventArgs eventArgs)
    {
        if (eventArgs.SpecialKey == ConsoleSpecialKey.ControlC)
        {
            eventArgs.Cancel = true;
            Message message = new ByeMessage();
            _tcpClient.SendMessage(message.Craft()); 
            ErrorHandler.ExitSuccess();
        }
        // else if (eventArgs.SpecialKey == ConsoleSpecialKey.ControlD)
        // {
        //     eventArgs.Cancel = true;
        //     Message message = new ByeMessage();
        //     Console.WriteLine(message.Craft());
        //     ErrorHandler.ExitSuccess();
        // }
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
                HandleCommandAuth(parameters);
                break;
            case "/join":
                HandleCommandJoin(parameters);
                break;
            case "/rename":
                HandleCommandRename(parameters);
                break;
            case "/help":
                HandleCommandHelp(parameters);
                break;
            default:
                ErrorHandler.InformUser($"Unknown command: {localCommand}. Try /help for list of supported commands.");
                return;
        }

    }
    
    private void HandleCommandAuth(string parameters)
    {
        string[] parametersSplit = parameters.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        if (parametersSplit.Length != 3)
        { 
            ErrorHandler.InformUser("Wrong parameters for command /auth. Try /help");
            return;
        }

        string username = parametersSplit[0];
        string secret = parametersSplit[1];
        _displayName = parametersSplit[2];

        Message message = new AuthMessage(username, _displayName, secret);
        _tcpClient.SendMessage(message.Craft()); 
    }
    
    private void HandleCommandJoin(string parameters)
    {
        string[] parametersSplit = parameters.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        
        if (parametersSplit.Length != 1)
        { 
            ErrorHandler.InformUser("Wrong parameters for command /join. Try /help");
            return;
        }

        string channelId = parametersSplit[0];
        Message message = new JoinMessage(channelId, _displayName);
        _tcpClient.SendMessage(message.Craft()); 
    }
    
    private void HandleCommandRename(string parameters)
    {
        string[] parametersSplit = parameters.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        
        if (parametersSplit.Length != 1)
        { 
            ErrorHandler.InformUser("Wrong parameters for command /rename. Try /help");
            return;
        }

        _displayName = parametersSplit[0];
        
        Console.WriteLine($"NEW DISPLAY NAME {_displayName}");
    }
    
    private void HandleCommandHelp(string parameters)
    {
        if (!string.IsNullOrEmpty(parameters))
        {
            ErrorHandler.InformUser($"Wrong parameters - command /help does not support any parameters");
            return;
        }
        Console.WriteLine("Available commands:");
        Console.WriteLine("\t/auth {Username} {Secret} {DisplayName} Sends AUTH message with the data provided from the command to the server");
        Console.WriteLine("\t/join {ChannelID}\t\t\tSends JOIN message with channel name from the command to the server");
        Console.WriteLine("\t/rename {DisplayName}\t\t\tchanges the display name of the user ");
        Console.WriteLine("\t/help\t\t\t\t\tprints this help message");
    }

       
    private void SendMessage(string messageContent)
    {
        Message message = new MsgMessage(_displayName, messageContent);
        _tcpClient.SendMessage(message.Craft());  
    }
}
