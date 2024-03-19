using ChatApp.Messages;

namespace ChatApp;

public static class MessageParser
{
    private static readonly Dictionary<string, Func<string[], Message?>> Parsers;

    static MessageParser()
    {
        Parsers = new Dictionary<string, Func<string[], Message?>>
        {
            { "ERR", ParseErrorMessage },
            { "MSG", ParseMessageMessage },
            { "REPLY", ParseReplyMessage },
            { "AUTH", ParseAuthMessage },
            { "JOIN", ParseJoinMessage },
            { "BYE", ParseByeMessage },
            { "CONFIRM", ParseConfirmMessage }
        };
    }

    public static Message? ParseMessage(string message)
    {
        foreach (var kvp in Parsers)
        {
            if (message.StartsWith(kvp.Key, StringComparison.OrdinalIgnoreCase))
            {
                string[] messageElements = message.Split(' ');
                return kvp.Value(messageElements);
            }
        }

        ErrorHandler.InternalError("Received unknown message");
        return null;
    }

    private static Message? ParseErrorMessage(string[] messageParts)
    {
        if (messageParts.Length != 5
            || !messageParts[1].Equals("FROM", StringComparison.OrdinalIgnoreCase)
            || !messageParts[3].Equals("IS", StringComparison.OrdinalIgnoreCase))
        {
            ErrorHandler.InternalError("Invalid ERR message format");
            return null;
        }

        return new ErrMessage(messageParts[2], messageParts[4]);
    }

    private static Message? ParseMessageMessage(string[] messageParts)
    {
        if (messageParts.Length != 5
            || !messageParts[1].Equals("FROM", StringComparison.OrdinalIgnoreCase)
            || !messageParts[3].Equals("IS", StringComparison.OrdinalIgnoreCase))
        {
            ErrorHandler.InternalError("Invalid MSG message format");
            return null;
        }

        return new MsgMessage(messageParts[2], messageParts[4]);
    }

    private static Message? ParseReplyMessage(string[] messageParts)
    {
        if (messageParts.Length != 4 || !messageParts[2].Equals("IS", StringComparison.OrdinalIgnoreCase))
        {
            ErrorHandler.InternalError("Invalid REPLY message format");
            return null;
        }

        if (!messageParts[1].Equals("OK", StringComparison.OrdinalIgnoreCase) &&
            !messageParts[1].Equals("NOK", StringComparison.OrdinalIgnoreCase))
        {
            ErrorHandler.InternalError("Invalid REPLY message format");
            return null;
        }

        bool isOk = messageParts[1].Equals("OK", StringComparison.OrdinalIgnoreCase);
        return new ReplyMessage(isOk, messageParts[3]);
    }

    private static Message? ParseAuthMessage(string[] messageParts)
    {
        if (messageParts.Length != 6 ||
            !messageParts[2].Equals("AS", StringComparison.OrdinalIgnoreCase) ||
            !messageParts[4].Equals("USING", StringComparison.OrdinalIgnoreCase))
        {
            ErrorHandler.InternalError("Invalid AUTH message format");
            return null;
        }

        return new AuthMessage(messageParts[1], messageParts[3], messageParts[5]);
    }

    private static Message? ParseJoinMessage(string[] messageParts)
    {
        if (messageParts.Length != 4 || !messageParts[2].Equals("AS", StringComparison.OrdinalIgnoreCase))
        {
            ErrorHandler.InternalError("Invalid JOIN message format");
            return null;
        }

        return new JoinMessage(messageParts[1], messageParts[3]);
    }

    private static Message? ParseByeMessage(string[] messageParts)
    {
        if (messageParts.Length != 1)
        {
            ErrorHandler.InternalError("Invalid BYE message format");
            return null;
        }

        return new ByeMessage();
    }

    private static Message? ParseConfirmMessage(string[] messageParts)
    {
        return null;
    }
}
    