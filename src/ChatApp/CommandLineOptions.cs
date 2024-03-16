namespace ChatApp;

using System;

public class CommandLineArguments
{
    public string? TransportProtocol { get; private set; }
    public string? ServerAddress { get; private set; }
    public ushort ServerPort { get; private set; } = 4567;
    public ushort UdpTimeout { get; private set; } = 250;
    public byte MaxRetransmissions { get; private set; } = 3;

    public CommandLineArguments(string[] args)
    {
        ParseArguments(args);
    }

    private void ParseArguments(string[] args)
    {
        for (int i = 0; i < args.Length; i += 2)
        {
            string arg = args[i];
            if (arg == "-h")
            {
                PrintHelp();
                Environment.Exit(0); 
            }
            string value = args[i + 1];

            switch (arg)
            {
                case "-t":
                    TransportProtocol = value;
                    break;
                case "-s":
                    ServerAddress = value;
                    break;
                case "-p":
                    ServerPort = ushort.Parse(value);
                    break;
                case "-d":
                    UdpTimeout = ushort.Parse(value);
                    break;
                case "-r":
                    MaxRetransmissions = byte.Parse(value);
                    break;
                default:
                    Console.WriteLine($"Unknown argument: {arg}");
                    PrintHelp();
                    Environment.Exit(1);
                    break;
            }
        }

        if (TransportProtocol == null || ServerAddress == null)
        {
            Console.WriteLine("Mandatory arguments missing.");
            PrintHelp();
            Environment.Exit(1);
        }
    }

    private static void PrintHelp()
    {
        Console.WriteLine("Usage: ipk24chat-client -t [tcp/udp] -s [server_address] [-p [server_port]] [-d [udp_timeout]] [-r [max_retransmissions]] [-h]");
        Console.WriteLine("Options:");
        Console.WriteLine("-t\tTransport protocol used for connection [TCP/UDP]");
        Console.WriteLine("-s\tServer IP or hostname");
        Console.WriteLine("-p\tServer port [default: 4567]");
        Console.WriteLine("-d\tUDP confirmation timeout [default: 250]");
        Console.WriteLine("-r\tMaximum number of UDP retransmissions [default: 3]");
        Console.WriteLine("-h\tPrints program help output and exits");
    }
}
