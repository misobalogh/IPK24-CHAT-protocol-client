using System;

namespace  ChatApp;

public class UserInputHandler
{
    private bool _exit;
    private TcpClientWrapper tcpClient = new TcpClientWrapper("127.0.0.1", 4567);
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
        }
        
        ErrorHandler.ExitSuccess();
    }

    private void OnCancelKeyPress(object? sender, ConsoleCancelEventArgs eventArgs)
    {
        eventArgs.Cancel = true;
        Console.WriteLine("Exit app");
        ErrorHandler.ExitSuccess();
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
        string displayName = parametersSplit[2];
        
        Console.WriteLine("command /auth");
        Console.WriteLine($"username {username}");
        Console.WriteLine($"secret {secret}");
        Console.WriteLine($"display name {displayName}");
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
        
        Console.WriteLine("command /join");
        Console.WriteLine($"Channel Id {channelId}");

    }
    
    private void HandleCommandRename(string parameters)
    {
        string[] parametersSplit = parameters.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        
        if (parametersSplit.Length != 1)
        { 
            ErrorHandler.InformUser("Wrong parameters for command /rename. Try /help");
            return;
        }

        string displayName = parametersSplit[0];
        
        Console.WriteLine("command /rename");
        Console.WriteLine($"Display Name {displayName}");
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

       
    private void SendMessage(string message)
    {
        tcpClient.SendMessage(message);   
        Console.WriteLine($"Send message: {message}");
    }
}
