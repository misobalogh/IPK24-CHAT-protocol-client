/*
 * File: ErrorHandler.cs
 * Description: Class for handling errors and exiting the application.
 * Author: Michal Balogh, xbalog06
 * Date: 30.03.2024
 */

using System;
using ChatApp.Enums;

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
