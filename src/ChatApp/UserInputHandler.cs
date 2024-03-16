using System;

namespace  ChatApp;

public class UserInputHandler
{
    public void ProcessInput()
    {
        string? input = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(input))
        {
            return;
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
                ErrorHandler.ExitWith($"Unknown command: {localCommand}. Try /help for list of supported commands.", ExitCode.UnknownCommand);
                break;
        }

    }
    
    private void HandleCommandAuth(string parameters)
    {
        string[] parametersSplit = parameters.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        if (parametersSplit.Length != 3)
        { 
            ErrorHandler.ExitWith("Wrong parameters for command /auth. Try /help", ExitCode.CommandWrongParams);
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
            ErrorHandler.ExitWith("Wrong parameters for command /join. Try /help", ExitCode.CommandWrongParams);
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
            ErrorHandler.ExitWith("Wrong parameters for command /rename. Try /help", ExitCode.CommandWrongParams);
        }

        string displayName = parametersSplit[0];
        
        Console.WriteLine("command /rename");
        Console.WriteLine($"Display Name {displayName}");
    }
    
    private void HandleCommandHelp(string parameters)
    {
        if (!string.IsNullOrEmpty(parameters))
        {
            ErrorHandler.ExitWith($"Wrong parameters - command /help does not support any parameters", ExitCode.CommandWrongParams);
        }
        Console.WriteLine("command /help");
    }

       
    private void SendMessage(string input)
    {
        Console.WriteLine($"Send message: {input}");
    }
}
