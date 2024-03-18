using ChatApp.Messages;

namespace ChatApp;


public class MessageParser
{
    public Message? ParseMessage(string message)
    {
        var messageParsers = new Dictionary<string, Func<string, Message?>>()
        {
            { "ERR FROM", ParseMessageErr },
            { "MSG FROM", ParseMessageMsg },
            { "REPLY", ParseMessageReply },
            { "AUTH", ParseMessageAuth },
            { "JOIN", ParseMessageJoin },
            { "BYE", ParseMessageBye },
            { "CONFIRM", ParseMessageConfirm }
        };

        foreach (var keyValuePair in messageParsers.Where(keyValuePair =>
                     message.StartsWith(keyValuePair.Key, StringComparison.OrdinalIgnoreCase)))
        {
            return keyValuePair.Value(message);
        }
        
        ErrorHandler.InformUser($"Received unknown message");
        // TODO: Handle wrong server message
        return null;

    }

    private Message? ParseMessageJoin(string message)
    {
        string[] parts = message.Split(' ');

        if (parts.Length != 4)
        {
            ErrorHandler.InformUser("Invalid JOIN message format");
            return null; 
        }

        if (!string.Equals(parts[0], "JOIN", StringComparison.OrdinalIgnoreCase) ||
            !string.Equals(parts[2], "AS", StringComparison.OrdinalIgnoreCase))
        {
            ErrorHandler.InformUser("Invalid AUTH message format");
            return null;
        }

        string channelId = parts[1];
        string displayName = parts[3];
        return new JoinMessage(channelId, displayName);
    }

    private Message? ParseMessageAuth(string message)
    {
        string[] parts = message.Split(' ');

        if (parts.Length != 6)
        {
            ErrorHandler.InformUser("Invalid AUTH message format");
            return null; 
        }

        if (!string.Equals(parts[0], "AUTH", StringComparison.OrdinalIgnoreCase) ||
            !string.Equals(parts[2], "AS", StringComparison.OrdinalIgnoreCase) ||
            !string.Equals(parts[4], "USING", StringComparison.OrdinalIgnoreCase))
        {
            ErrorHandler.InformUser("Invalid AUTH message format");
            return null;
        }

        string username = parts[1];
        string displayName = parts[3];
        string secret = parts[5];
        return new AuthMessage(username, displayName, secret);
    }

    private Message? ParseMessageConfirm(string message)
    {
        return null;
    }

    private Message? ParseMessageBye(string message)
    {
        if (!string.Equals(message, "BYE", StringComparison.OrdinalIgnoreCase))
        {   
            ErrorHandler.InformUser("Invalid BYE message format");
            return null;
        }
        
        return new ByeMessage();
    }

    private Message? ParseMessageReply(string message)
    {
        string[] parts = message.Split(' ');

        if (parts.Length != 4)
        {
            ErrorHandler.InformUser("Invalid REPLY message format");
            return null; 
        }

        if (!string.Equals(parts[0], "REPLY", StringComparison.OrdinalIgnoreCase) ||
            !(string.Equals(parts[1], "OK", StringComparison.OrdinalIgnoreCase) ||
              string.Equals(parts[1], "NOK", StringComparison.OrdinalIgnoreCase)) ||
              !string.Equals(parts[2], "IS", StringComparison.OrdinalIgnoreCase))
        {
            ErrorHandler.InformUser("Invalid REPLY message format");
            return null;
        }

        bool isOk = parts[1] == "OK";
        string messageContent = parts[3];
        return new ReplyMessage(isOk, messageContent);
    }

    private Message? ParseMessageErr(string message)
    {
        string[] parts = message.Split(' ');

        if (parts.Length != 5)
        {
            ErrorHandler.InformUser("Invalid ERR message format");
            return null; 
        }

        if (!string.Equals(parts[0], "ERR", StringComparison.OrdinalIgnoreCase) ||
            !string.Equals(parts[1], "FROM", StringComparison.OrdinalIgnoreCase) ||
            !string.Equals(parts[3], "IS", StringComparison.OrdinalIgnoreCase))
        {
            ErrorHandler.InformUser("Invalid ERR message format");
            return null;
        }

        string displayName = parts[2];
        string messageContent = parts[4];
        return new ErrMessage(displayName, messageContent);
    }

    private Message? ParseMessageMsg(string message)
    {
        string[] parts = message.Split(' ');

        if (parts.Length != 5)
        {
            ErrorHandler.InformUser("Invalid MSG message format");
            return null; 
        }

        if (!string.Equals(parts[0], "MSG", StringComparison.OrdinalIgnoreCase) ||
            !string.Equals(parts[1], "FROM", StringComparison.OrdinalIgnoreCase) ||
            !string.Equals(parts[3], "IS", StringComparison.OrdinalIgnoreCase))
        {
            ErrorHandler.InformUser("Invalid MSG message format");
            return null;
        }

        string displayName = parts[2];
        string messageContent = parts[4];
        return new MsgMessage(displayName, messageContent);
    }
}