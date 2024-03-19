using System;
using ChatApp.Enums;

namespace ChatApp; 

internal static class Program
{
    private static void Main(string[] args)
    {

        var cmdArgs = new CommandLineArguments(args);

        // Console.WriteLine($"Transport Protocol: {cmdArgs.TransportProtocol}");
        // Console.WriteLine($"Server Address: {cmdArgs.ServerAddress}");
        // Console.WriteLine($"Server Port: {cmdArgs.ServerPort}");
        // Console.WriteLine($"UDP Timeout: {cmdArgs.UdpTimeout}");
        // Console.WriteLine($"Max Retransmissions: {cmdArgs.MaxRetransmissions}");
        
        UserInputHandler userInputHandler = new UserInputHandler();
        userInputHandler.ProcessInput().Wait(); 
    }
}

