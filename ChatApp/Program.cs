using System;
using ChatApp.Enums;

namespace ChatApp; 

internal static class Program
{
    private static void Main(string[] args)
    {

        var cmdArgs = new CommandLineArguments(args);

        Console.WriteLine($"Transport Protocol: {cmdArgs.TransportProtocol}");
        Console.WriteLine($"Server Address: {cmdArgs.ServerAddress}");
        Console.WriteLine($"Server Port: {cmdArgs.ServerPort}");
        Console.WriteLine($"UDP Timeout: {cmdArgs.UdpTimeout}");
        Console.WriteLine($"Max Retransmissions: {cmdArgs.MaxRetransmissions}");


        // var clientState = new ClientState();
        // Console.WriteLine($"State: {clientState.GetCurrentState()}");
        //
        // clientState.NextState(MessageType.None, MessageType.Auth);
        // Console.WriteLine($"State: {clientState.GetCurrentState()}");
        //
        // clientState.NextState(MessageType.NotReply, MessageType.Auth);
        // Console.WriteLine($"State: {clientState.GetCurrentState()}");
        //
        // clientState.NextState(MessageType.None, MessageType.Bye);
        // Console.WriteLine($"State: {clientState.GetCurrentState()}");
        
        // var usrInput = new UserInputHandler();
        // usrInput.ProcessInput();

        var msgParser = new MessageParser();
        var msg = msgParser.ParseMessage("MSG FROM user1 IS hello");
        Console.WriteLine(msg?.Craft());
    }
}

