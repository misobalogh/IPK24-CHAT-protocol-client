using System;

namespace ChatApp; 

static class Program
{
    private static void Main(string[] args)
    {
        var cmdArgs = new CommandLineArguments(args);

        Console.WriteLine($"Transport Protocol: {cmdArgs.TransportProtocol}");
        Console.WriteLine($"Server Address: {cmdArgs.ServerAddress}");
        Console.WriteLine($"Server Port: {cmdArgs.ServerPort}");
        Console.WriteLine($"UDP Timeout: {cmdArgs.UdpTimeout}");
        Console.WriteLine($"Max Retransmissions: {cmdArgs.MaxRetransmissions}");

        var usrInput = new UserInputHandler();
        usrInput.ProcessInput();


    }
}

