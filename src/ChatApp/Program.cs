/*
 * File: Program.cs
 * Description: Main entry point of the application.
 * Author: Michal Balogh, xbalog06
 * Date: 30.03.2024
 */

using System.Text;
using ChatApp.Enums;
using ChatApp.Messages;

namespace ChatApp; 

internal static class Program
{
    private static void Main(string[] args)
    {
        var cmdArgs = new CommandLineArguments(args);
        
        UserInputHandler userInputHandler = new UserInputHandler(
            cmdArgs.TransportProtocol,
            cmdArgs.ServerAddress,
            cmdArgs.ServerPort,
            cmdArgs.UdpTimeout,
            cmdArgs.MaxRetransmissions
            );
        
        userInputHandler.ProcessInput().Wait();
    }
}

