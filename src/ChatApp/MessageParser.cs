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
            { MessageTypeByte.Auth, ParseReplyMessage },
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
        throw new NotImplementedException();
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
        throw new NotImplementedException();
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
        throw new NotImplementedException();
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
        throw new NotImplementedException();
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
        throw new NotImplementedException();
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

        ushort messageId = GetMessageIdFromBytes(messageParts);
        return new ByeMessage(messageId);
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
    
    private static Message? ParseConfirmMessage(byte[] messageParts)
    {
        throw new NotImplementedException();
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
}
    