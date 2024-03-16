namespace ChatApp;

public static class ErrorHandler
{
    public static void Error(string message, ErrorCode errorCode)
    {
        Console.Error.WriteLine($"ERROR: {message}");
        Environment.Exit((int)errorCode);
    }
}
