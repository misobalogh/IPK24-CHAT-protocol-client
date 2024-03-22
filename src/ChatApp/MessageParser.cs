using System.Text;
using ChatApp.Messages;
using ChatApp.Enums;

namespace ChatApp;

public static class MessageParser
{
    private static readonly Dictionary<string, Func<string[], Message?>> TcpParsers;
    private static readonly Dictionary<MessageTypeByte, Func<byte[], Message?>> UdpParsers;

    static MessageParser()
    {
        TcpParsers = new Dictionary<string, Func<string[], Message?>>
        {
            { "ERR FROM", ParseErrMessage },
            { "MSG FROM", ParseMsgMessage },
            { "REPLY", ParseReplyMessage },
            { "AUTH", ParseAuthMessage },
            { "JOIN", ParseJoinMessage },
            { "BYE", ParseByeMessage },
            { "CONFIRM", ParseConfirmMessage }
        };

        UdpParsers = new Dictionary<MessageTypeByte, Func<byte[], Message?>>
        {
            { MessageTypeByte.Err, ParseErrMessage },
            { MessageTypeByte.Msg, ParseMsgMessage },
            { MessageTypeByte.Reply, ParseReplyMessage },
            { MessageTypeByte.Auth, ParseAuthMessage },
            { MessageTypeByte.Join, ParseJoinMessage },
            { MessageTypeByte.Bye, ParseByeMessage },
            { MessageTypeByte.Confirm, ParseConfirmMessage }
        };
    }

    // method for tcp variant, works with strings
    public static Message? ParseMessage(string message)
    {
        foreach (var kvp in TcpParsers)
        {
            if (message.StartsWith(kvp.Key, StringComparison.OrdinalIgnoreCase))
            {
                var messageElements = message.Split(' ');
                return kvp.Value(messageElements);
            }
        }

        ErrorHandler.InformUser("Received unknown message");
        return null;
    }

    // overloaded method for udp, works with array of bytes
    public static Message? ParseMessage(byte[] message)
    {
        if (message.Length != 0)
        {
            foreach (var kvp in UdpParsers)
            {
                if (message[0] == (byte)kvp.Key)
                {

                    return kvp.Value(message);
                }
            }
        }

        ErrorHandler.InformUser("Received unknown message");
        return null;
    }
    
    
    private static Message? ParseErrMessage(byte[] messageParts)
    {
        
        if (messageParts.Length < 7 || messageParts[^1] != 0)
        {
            ErrorHandler.InformUser("Received invalid ERR message");
            return null;
        }

        int displayNameEndIndex = Array.IndexOf(messageParts, (byte)0, 3);

        if (displayNameEndIndex == -1 || displayNameEndIndex == messageParts.Length - 1 || messageParts[displayNameEndIndex] != 0)
        {
            ErrorHandler.InformUser("Received invalid ERR message");
            return null;
        }

        ushort messageId = GetMessageIdFromBytes(messageParts[1..3]);
        string displayName = GetStringFromBytes(messageParts, 3, out displayNameEndIndex);
        string messageContents = GetStringFromBytes(messageParts, displayNameEndIndex + 1, out _);

        return new ErrMessage(displayName, messageContents, messageId);
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

    private static Message? ParseMsgMessage(byte[] messageParts)
    {
        if (messageParts.Length < 7 || messageParts[^1] != 0)
        {
            ErrorHandler.InformUser("Received invalid MSG message");
            return null;
        }

        int displayNameEndIndex = Array.IndexOf(messageParts, (byte)0, 3);

        if (displayNameEndIndex == -1 || displayNameEndIndex == messageParts.Length - 1 || messageParts[displayNameEndIndex] != 0)
        {
            ErrorHandler.InformUser("Received invalid MSG message");
            return null;
        }

        ushort messageId = GetMessageIdFromBytes(messageParts[1..3]);
        string displayName = GetStringFromBytes(messageParts, 3, out displayNameEndIndex);
        string messageContents = GetStringFromBytes(messageParts, displayNameEndIndex + 1, out _);

        return new MsgMessage(displayName, messageContents, messageId);
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
    
    private static Message? ParseReplyMessage(byte[] messageParts)
    {
        if (messageParts.Length < 8 || messageParts[^1] != 0)
        {
            ErrorHandler.InformUser("Received invalid REPLY message");
            return null;
        }

        ushort messageId = GetMessageIdFromBytes(messageParts[1..3]);
        bool result = messageParts[3] == 1;
        ushort refMessageId = GetMessageIdFromBytes(messageParts[4..6]);
    
        int messageContentsStartIndex = 6;

        string messageContents = GetStringFromBytes(messageParts, messageContentsStartIndex, out _);

        return new ReplyMessage(result, messageContents, refMessageId, messageId);
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
    
    private static Message? ParseAuthMessage(byte[] messageParts)
    {
        if (messageParts.Length < 9 || messageParts[^1] != 0)
        {
            ErrorHandler.InformUser("Received invalid AUTH message");
            return null;
        }

        int usernameEndIndex = Array.IndexOf(messageParts, (byte)0, 3);
        if (usernameEndIndex == -1 || usernameEndIndex == messageParts.Length - 1 || messageParts[usernameEndIndex] != 0)
        {
            ErrorHandler.InformUser("Received invalid AUTH message");
            return null;
        }

        int displayNameStartIndex = usernameEndIndex + 1;
        int displayNameEndIndex = Array.IndexOf(messageParts, (byte)0, displayNameStartIndex);
        if (displayNameEndIndex == -1 || displayNameEndIndex == messageParts.Length - 1 || messageParts[displayNameEndIndex] != 0)
        {
            ErrorHandler.InformUser("Received invalid AUTH message");
            return null;
        }

        ushort messageId = GetMessageIdFromBytes(messageParts[1..3]);
        string username = GetStringFromBytes(messageParts, 3, out usernameEndIndex);
        string displayName = GetStringFromBytes(messageParts, displayNameStartIndex, out displayNameEndIndex);
        string secret = GetStringFromBytes(messageParts, displayNameEndIndex + 1, out _);

        return new AuthMessage(username, displayName, secret, messageId);
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
    
    private static Message? ParseJoinMessage(byte[] messageParts)
    {
        if (messageParts.Length < 7 || messageParts[^1] != 0)
        {
            ErrorHandler.InformUser("Received invalid JOIN message");
            return null;
        }

        int channelIdEndIndex = Array.IndexOf(messageParts, (byte)0, 3);
        if (channelIdEndIndex == -1 || channelIdEndIndex == messageParts.Length - 1 || messageParts[channelIdEndIndex] != 0)
        {
            ErrorHandler.InformUser("Received invalid JOIN message");
            return null;
        }
        
        int displayNameStartIndex = channelIdEndIndex + 1;

        ushort messageId = GetMessageIdFromBytes(messageParts[1..3]);
        string channelId = GetStringFromBytes(messageParts, 3, out channelIdEndIndex);
        string displayName = GetStringFromBytes(messageParts, displayNameStartIndex, out _);

        return new JoinMessage(channelId, displayName, messageId);
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
    
    private static Message? ParseByeMessage(byte[] messageParts)
    {
        if (messageParts.Length != 3)
        {
            ErrorHandler.InformUser("Received invalid BYE message");
            return null;
        }

        var messageIdBytes = messageParts[1..];
        ushort messageId = GetMessageIdFromBytes(messageIdBytes);
        return new ByeMessage(messageId);
    }

    private static Message? ParseByeMessage(string[] messageParts)
    {
        if (messageParts.Length != 1)
        {
            ErrorHandler.InformUser("Receive invalid BYE message");
            return null;
        }

        return new ByeMessage();
    }
    
    private static Message? ParseConfirmMessage(byte[] messageParts)
    {
        if (messageParts.Length != 3)
        {
            ErrorHandler.InformUser("Received invalid CONFIRM message");
            return null;
        }

        var messageIdBytes = messageParts[1..];
        ushort messageId = GetMessageIdFromBytes(messageIdBytes);
        return new ConfirmMessage(messageId);
    }

    private static Message? ParseConfirmMessage(string[] messageParts)
    {
        return null;
    }
    
    
    private static ushort GetMessageIdFromBytes(byte[] messageIdBytes)
    {
        if (BitConverter.IsLittleEndian)
        {
            Array.Reverse(messageIdBytes);
        }
        return BitConverter.ToUInt16(messageIdBytes, 0);
    }

    private static string GetStringFromBytes(byte[] bytes, int startIndex, out int endIndex)
    {
        endIndex = Array.IndexOf(bytes, (byte)0, startIndex);
        if (endIndex == -1)
        {
            endIndex = bytes.Length - 1;
        }
        return Encoding.ASCII.GetString(bytes[startIndex..endIndex]);
    }
}
    