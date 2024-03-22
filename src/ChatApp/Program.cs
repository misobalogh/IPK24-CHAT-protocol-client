using System.Text;
using ChatApp.Messages;

namespace ChatApp; 

internal static class Program
{
    private static void Main(string[] args)
    {
        var cmdArgs = new CommandLineArguments(args);
        
        var msg = MessageParser.ParseMessage(new ReplyMessage(false ,"abc", 11, 22).CraftUdp());
        Console.WriteLine(msg.CraftTcp());
        Console.WriteLine(msg?.MessageId);
        // UserInputHandler userInputHandler = new UserInputHandler(
        //     cmdArgs.TransportProtocol,
        //     cmdArgs.ServerAddress,
        //     cmdArgs.ServerPort,
        //     cmdArgs.UdpTimeout,
        //     cmdArgs.MaxRetransmissions
        //     );
        //
        // userInputHandler.ProcessInput().Wait();

    }
}

