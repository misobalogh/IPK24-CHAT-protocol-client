using System;

namespace ChatApp;

public static class ErrorHandler
{
    public static void ExitWith(string message, ExitCode exitCode)
    {
        Console.Error.WriteLine($"ERROR: {message}");
        Environment.Exit((int)exitCode);
    }

    public static void ExitSuccess()
    {
        Environment.Exit((int)ExitCode.Success);
    }

    public static void InformUser(string message)
    {
        Console.Error.WriteLine($"WARNING: {message}");
    }
}
