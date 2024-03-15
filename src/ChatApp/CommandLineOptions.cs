using System;
using System.CommandLine;
using System.CommandLine.Invocation;

namespace ChatApp;

public class CommandLineOptions
{
    public string Transport { get; set; } = "tcp";
    public string Server { get; set; } = "localhost";
    public ushort Port { get; set; } = 4567;
    public ushort Timeout { get; set; } = 250;
    public byte Retransmissions { get; set; } = 3;
    public bool Help { get; set; }

    public static CommandLineOptions Parse(string[] args)
    {
        var options = new CommandLineOptions();
        var rootCommand = new RootCommand
        {
            new Option<string>(
                "--transport",
                description: "Transport protocol used for connection (tcp or udp)",
                getDefaultValue: () => "tcp"
            ),
            new Option<string>(
                "--server",
                description: "Server IP or hostname",
                getDefaultValue: () => "localhost"
            ),
            new Option<ushort>(
                "--port",
                description: "Server port",
                getDefaultValue: () => 4567
            ),
            new Option<ushort>(
                "--timeout",
                description: "UDP confirmation timeout",
                getDefaultValue: () => 250
            ),
            new Option<byte>(
                "--retransmissions",
                description: "Maximum number of UDP retransmissions",
                getDefaultValue: () => 3
            ),
            new Option<bool>(
                "--help",
                description: "Prints program help output and exits"
            )
        };

        rootCommand.Handler = CommandHandler.Create<string, string, ushort, ushort, byte, bool>((transport, server, port, timeout, retransmissions, help) =>
        {
            options.Transport = transport;
            options.Server = server;
            options.Port = port;
            options.Timeout = timeout;
            options.Retransmissions = retransmissions;
            options.Help = help;
        });

        rootCommand.Invoke(args);
        return options;
    }
}
