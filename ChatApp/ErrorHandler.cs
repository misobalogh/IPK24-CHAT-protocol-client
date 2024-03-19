using System;

namespace ChatApp;

public static class ErrorHandler
{
    public static void ExitWith(string message, ExitCode exitCode)
    {
        InternalError(message);
        Environment.Exit((int)exitCode);
    }

    public static void ExitSuccess()
    {
        Environment.Exit((int)ExitCode.Success);
    }

    public static void InternalError(string messageContent)
    {
        Console.Error.WriteLine($"ERR: {messageContent}");
    }
}
