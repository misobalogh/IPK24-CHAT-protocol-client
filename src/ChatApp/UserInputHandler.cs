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
        string[] splitCommand = command.Split(' ', 2);
        string localCommand = splitCommand[0];
        string? parameters = splitCommand.Length > 1 ? splitCommand[1] : null;

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
                ErrorHandler.Error($"Unknown command: {localCommand}. Try /help for lst of supported commands.", ErrorCode.UnknownCommand);
                break;
        }

    }
    
    private void HandleCommandAuth(string? parameters)
    {
        Console.WriteLine("command /auth");
    }
    
    private void HandleCommandJoin(string? parameters)
    {
        Console.WriteLine("command /join");
    }
    
    private void HandleCommandRename(string? parameters)
    {
        Console.WriteLine("command /rename");
    }
    
    private void HandleCommandHelp(string? parameters)
    {
        Console.WriteLine("command /help");
    }
    
    private void SendMessage(string input)
    {
        
    }
}
