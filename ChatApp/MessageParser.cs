using ChatApp.Messages;

namespace ChatApp;

public static class MessageParser
{
    private static readonly Dictionary<string, Func<string[], Message?>> Parsers;

    static MessageParser()
    {
        Parsers = new Dictionary<string, Func<string[], Message?>>
        {
            { "ERR FROM", ParseErrMessage },
            { "MSG FROM", ParseMsgMessage },
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

        ErrorHandler.InformUser("Received unknown message");
        return null;
    }

    private static Message? ParseErrMessage(string[] messageParts)
    {
        if (messageParts.Length >= 5 
            && MessageGrammar.IsDname(messageParts[2])
            && MessageGrammar.IsComponentIS(messageParts[3])
            && MessageGrammar.IsContent(messageParts[4]))
        {
            return new ErrMessage(messageParts[2], string.Join(" ", messageParts[4..]));
        }
        
        ErrorHandler.InformUser("Received unknown message");
        return null;

    }

    private static Message? ParseMsgMessage(string[] messageParts)
    {
        if (messageParts.Length >= 5 
            && MessageGrammar.IsDname(messageParts[2])
            && MessageGrammar.IsComponentIS(messageParts[3])
            && MessageGrammar.IsContent(messageParts[4]))
        {
            return new MsgMessage(messageParts[2], string.Join(" ", messageParts[4..]));
        }
        
        ErrorHandler.InformUser("Received unknown message");
        return null;

    }

    private static Message? ParseReplyMessage(string[] messageParts)
    {
       if (messageParts.Length >= 4 
            && MessageGrammar.IsComponentOKorNOK(messageParts[1])
            && MessageGrammar.IsComponentIS(messageParts[2])
            && MessageGrammar.IsContent(messageParts[3]))
        {
            bool isOk = messageParts[1].Equals("OK", StringComparison.OrdinalIgnoreCase);
            return new ReplyMessage(isOk, string.Join(" ", messageParts[3..]));
        }

        ErrorHandler.InformUser("Invalid REPLY message format");
        return null;
    }

    private static Message? ParseAuthMessage(string[] messageParts)
    {
        if (messageParts.Length == 6 
            && MessageGrammar.IsId(messageParts[1]) 
            && MessageGrammar.IsComponentAS(messageParts[2]) 
            && MessageGrammar.IsDname(messageParts[3])
            && MessageGrammar.IsComponentUSING(messageParts[4]) 
            && MessageGrammar.IsSecret(messageParts[5]))
        {
            return new AuthMessage(messageParts[1], messageParts[3], messageParts[5]);
        }
        
        ErrorHandler.InformUser("Received unknown message");
        return null;

    }

    private static Message? ParseJoinMessage(string[] messageParts)
    {
        if (messageParts.Length == 4 
            && MessageGrammar.IsId(messageParts[1])
            && MessageGrammar.IsComponentAS(messageParts[2])
            && MessageGrammar.IsDname(messageParts[3]))
        {
            return new JoinMessage(messageParts[1], messageParts[3]);
        }
        
        ErrorHandler.InformUser("Received unknown message");
        return null;

    }

    private static Message? ParseByeMessage(string[] messageParts)
    {
        if (messageParts.Length != 1)
        {
            ErrorHandler.InformUser("Received unknown message");
            return null;
        }

        return new ByeMessage();
    }

    private static Message? ParseConfirmMessage(string[] messageParts)
    {
        return null;
    }
}
    