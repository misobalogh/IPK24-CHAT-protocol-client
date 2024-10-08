/*
 * File: MessageParser.cs
 * Description: Parser for received messages.
 * Author: Michal Balogh, xbalog06
 * Date: 30.03.2024
 */

using System.Text;
using ChatApp.Messages;
using ChatApp.Enums;

namespace ChatApp;

/// <summary>
/// Utility class for parsing messages received from the server.
/// </summary>
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

    /// <summary>
    /// Parses a message received from the server in string format (used in TCP variant).
    /// </summary>
    /// <param name="message">The message received from the server as a string.</param>
    /// <returns>The parsed Message object, or null if parsing fails.</returns>
    public static Message? ParseMessage(string message)
    {
        foreach (var kvp in TcpParsers)
        {
            // match message with possible messages in dictionary
            if (message.StartsWith(kvp.Key, StringComparison.OrdinalIgnoreCase))
            {
                var messageElements = message.Split(' ');
                // invoke method that tries to parse the message
                return kvp.Value(messageElements);
            }
        }

        ErrorHandler.InternalError("Received unknown message");
        return null;
    }

    /// <summary>
    /// Parses a message received from the server in byte array format (used in UDP variant).
    /// </summary>
    /// <param name="message">The message received from the server as a byte array.</param>
    /// <returns>The parsed Message object, or null if parsing fails.</returns>
    public static Message? ParseMessage(byte[] message)
    {
        if (message.Length != 0)
        {
            foreach (var kvp in UdpParsers)
            {
                // match message with possible messages in dictionary
                if (message[0] == (byte)kvp.Key)
                {
                    // invoke method that tries to parses the message
                    return kvp.Value(message);
                }
            }
        }

        ErrorHandler.InternalError("Received unknown message");
        return null;
    }
    
    /// <remarks>
    /// The correct format of the ERR message is:
    ///   1 byte       2 bytes
    /// +--------+--------+--------+-------~~------+---+--------~~---------+---+
    /// |  0xFE  |    MessageID    |  DisplayName  | 0 |  MessageContents  | 0 |
    /// +--------+--------+--------+-------~~------+---+--------~~---------+---+
    /// </remarks>
    private static Message? ParseErrMessage(byte[] messageParts)
    {
        if (messageParts.Length < 7 || messageParts[^1] != 0)
        {
            ErrorHandler.InternalError("Received invalid ERR message");
            return null;
        }

        int displayNameEndIndex = Array.IndexOf(messageParts, (byte)0, 3);

        if (displayNameEndIndex == -1 || displayNameEndIndex == messageParts.Length - 1 || messageParts[displayNameEndIndex] != 0)
        {
            ErrorHandler.InternalError("Received invalid ERR message");
            return null;
        }

        ushort messageId = GetMessageIdFromBytes(messageParts[1..3]);
        string displayName = GetStringFromBytes(messageParts, 3, out displayNameEndIndex);
        string messageContents = GetStringFromBytes(messageParts, displayNameEndIndex + 1, out _);

        return new ErrMessage(displayName, messageContents, messageId);
    }
    
    /// <remarks>
    /// The correct format of the ERR message is:
    /// ERR FROM {DisplayName} IS {MessageContent}\r\n
    /// </remarks>
    private static Message? ParseErrMessage(string[] messageParts)
    {
        if (messageParts.Length >= 5 
            && MessageGrammar.IsDname(messageParts[2])
            && MessageGrammar.IsComponentIS(messageParts[3])
            && MessageGrammar.IsContent(messageParts[4]))
        {
            return new ErrMessage(messageParts[2], string.Join(" ", messageParts[4..]));
        }
        
        ErrorHandler.InternalError("Received unknown message");
        return null;
    }
    
    /// <remarks>
    /// The correct format of the MSG message is:
    ///   1 byte       2 bytes
    /// +--------+--------+--------+-------~~------+---+--------~~---------+---+
    /// |  0x04  |    MessageID    |  DisplayName  | 0 |  MessageContents  | 0 |
    /// +--------+--------+--------+-------~~------+---+--------~~---------+---+
    /// </remarks>
    private static Message? ParseMsgMessage(byte[] messageParts)
    {
        if (messageParts.Length < 7 || messageParts[^1] != 0)
        {
            ErrorHandler.InternalError("Received invalid MSG message");
            return null;
        }

        int displayNameEndIndex = Array.IndexOf(messageParts, (byte)0, 3);

        if (displayNameEndIndex == -1 || displayNameEndIndex == messageParts.Length - 1 || messageParts[displayNameEndIndex] != 0)
        {
            ErrorHandler.InternalError("Received invalid MSG message");
            return null;
        }

        ushort messageId = GetMessageIdFromBytes(messageParts[1..3]);
        string displayName = GetStringFromBytes(messageParts, 3, out displayNameEndIndex);
        string messageContents = GetStringFromBytes(messageParts, displayNameEndIndex + 1, out _);

        return new MsgMessage(displayName, messageContents, messageId);
    }
    
    /// <remarks>
    /// The correct format of the MSG message is:
    /// MSG FROM {DisplayName} IS {MessageContent}\r\n
    /// </remarks>
    private static Message? ParseMsgMessage(string[] messageParts)
    {
        if (messageParts.Length >= 5
            && MessageGrammar.IsDname(messageParts[2])
            && MessageGrammar.IsComponentIS(messageParts[3])
            && MessageGrammar.IsContent(messageParts[4]))
        {
            return new MsgMessage(messageParts[2], string.Join(" ", messageParts[4..]));
        }

        ErrorHandler.InternalError("Received unknown message");
        return null;
    }
    
    /// <remarks>
    /// The correct format of the REPLY message is:
    ///  1 byte       2 bytes       1 byte       2 bytes
    /// +--------+--------+--------+--------+--------+--------+--------~~---------+---+
    /// |  0x01  |    MessageID    | Result |  Ref_MessageID  |  MessageContents  | 0 |
    /// +--------+--------+--------+--------+--------+--------+--------~~---------+---+
    /// </remarks>
    private static Message? ParseReplyMessage(byte[] messageParts)
    {
        if (messageParts.Length < 8 || messageParts[^1] != 0)
        {
            ErrorHandler.InternalError("Received invalid REPLY message");
            return null;
        }

        ushort messageId = GetMessageIdFromBytes(messageParts[1..3]);
        bool result = messageParts[3] == 1;
        ushort refMessageId = GetMessageIdFromBytes(messageParts[4..6]);
    
        int messageContentsStartIndex = 6;

        string messageContents = GetStringFromBytes(messageParts, messageContentsStartIndex, out _);

        return new ReplyMessage(result, messageContents, messageId: messageId, refMessageId: refMessageId);
    }

    
    /// <remarks>
    /// The correct format of the REPLY message is:
    /// REPLY {"OK"|"NOK"} IS {MessageContent}\r\n
    /// </remarks>
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

        ErrorHandler.InternalError("Invalid REPLY message format");
        return null;
    }
    
    /// <remarks>
    /// The correct format of the AUTH message is:
    ///   1 byte       2 bytes
    /// +--------+--------+--------+-----~~-----+---+-------~~------+---+----~~----+---+
    /// |  0x02  |    MessageID    |  Username  | 0 |  DisplayName  | 0 |  Secret  | 0 |
    /// +--------+--------+--------+-----~~-----+---+-------~~------+---+----~~----+---+
    /// </remarks>
    private static Message? ParseAuthMessage(byte[] messageParts)
    {
        if (messageParts.Length < 9 || messageParts[^1] != 0)
        {
            ErrorHandler.InternalError("Received invalid AUTH message");
            return null;
        }

        int usernameEndIndex = Array.IndexOf(messageParts, (byte)0, 3);
        if (usernameEndIndex == -1 || usernameEndIndex == messageParts.Length - 1 || messageParts[usernameEndIndex] != 0)
        {
            ErrorHandler.InternalError("Received invalid AUTH message");
            return null;
        }

        int displayNameStartIndex = usernameEndIndex + 1;
        int displayNameEndIndex = Array.IndexOf(messageParts, (byte)0, displayNameStartIndex);
        if (displayNameEndIndex == -1 || displayNameEndIndex == messageParts.Length - 1 || messageParts[displayNameEndIndex] != 0)
        {
            ErrorHandler.InternalError("Received invalid AUTH message");
            return null;
        }

        ushort messageId = GetMessageIdFromBytes(messageParts[1..3]);
        string username = GetStringFromBytes(messageParts, 3, out usernameEndIndex);
        string displayName = GetStringFromBytes(messageParts, displayNameStartIndex, out displayNameEndIndex);
        string secret = GetStringFromBytes(messageParts, displayNameEndIndex + 1, out _);

        return new AuthMessage(username, displayName, secret, messageId);
    }


    /// <remarks>
    /// The correct format of the AUTH message is:
    /// AUTH {Username} AS {DisplayName} USING {Secret}\r\n
    /// </remarks>
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
        
        ErrorHandler.InternalError("Received unknown message");
        return null;

    }
    /// <remarks>
    /// The correct format of the JOIN message is:
    ///   1 byte       2 bytes
    /// +--------+--------+--------+-----~~-----+---+-------~~------+---+
    /// |  0x03  |    MessageID    |  ChannelID | 0 |  DisplayName  | 0 |
    /// +--------+--------+--------+-----~~-----+---+-------~~------+---+
    /// </remarks>
    private static Message? ParseJoinMessage(byte[] messageParts)
    {
        if (messageParts.Length < 7 || messageParts[^1] != 0)
        {
            ErrorHandler.InternalError("Received invalid JOIN message");
            return null;
        }

        int channelIdEndIndex = Array.IndexOf(messageParts, (byte)0, 3);
        if (channelIdEndIndex == -1 || channelIdEndIndex == messageParts.Length - 1 || messageParts[channelIdEndIndex] != 0)
        {
            ErrorHandler.InternalError("Received invalid JOIN message");
            return null;
        }

        ushort messageId = GetMessageIdFromBytes(messageParts[1..3]);
        string channelId = GetStringFromBytes(messageParts, 3, out channelIdEndIndex);
        string displayName = GetStringFromBytes(messageParts, channelIdEndIndex + 1, out _);

        return new JoinMessage(channelId, displayName, messageId);
    }


    /// <remarks>
    /// The correct format of the error message is:
    /// JOIN {ChannelID} AS {DisplayName}\r\n
    /// </remarks>
    private static Message? ParseJoinMessage(string[] messageParts)
    {
        if (messageParts.Length == 4 
            && MessageGrammar.IsId(messageParts[1])
            && MessageGrammar.IsComponentAS(messageParts[2])
            && MessageGrammar.IsDname(messageParts[3]))
        {
            return new JoinMessage(messageParts[1], messageParts[3]);
        }
        
        ErrorHandler.InternalError("Received unknown message");
        return null;

    }
    
    /// <remarks>
    /// The correct format of the error message is:
    ///   1 byte       2 bytes
    /// +--------+--------+--------+
    /// |  0xFF  |    MessageID    |
    /// +--------+--------+--------+
    /// </remarks>
    private static Message? ParseByeMessage(byte[] messageParts)
    {
        if (messageParts.Length != 3)
        {
            ErrorHandler.InternalError("Received invalid BYE message");
            return null;
        }

        var messageIdBytes = messageParts[1..];
        ushort messageId = GetMessageIdFromBytes(messageIdBytes);
        return new ByeMessage(messageId);
    }

    /// <remarks>
    /// The correct format of the error message is:
    /// BYE\r\n
    /// </remarks>
    private static Message? ParseByeMessage(string[] messageParts)
    {
        if (messageParts.Length != 1)
        {
            ErrorHandler.InternalError("Receive invalid BYE message");
            return null;
        }

        return new ByeMessage();
    }
    
    /// <remarks>
    /// The correct format of the error message is:
    ///   1 byte       2 bytes
    /// +--------+--------+--------+
    /// |  0x00  |  Ref_MessageID  |
    /// +--------+--------+--------+
    /// </remarks>

    private static Message? ParseConfirmMessage(byte[] messageParts)
    {
        if (messageParts.Length != 3)
        {
            ErrorHandler.InternalError("Received invalid CONFIRM message");
            return null;
        }

        var messageIdBytes = messageParts[1..];
        ushort messageId = GetMessageIdFromBytes(messageIdBytes);
        return new ConfirmMessage(messageId);
    }

    /// <remarks>
    /// Unused in TCP
    /// </remarks>
    private static Message? ParseConfirmMessage(string[] messageParts)
    {
        return null;
    }
    
    /// <summary>
    /// Converts bytes to message ID in correct endianness
    /// </summary>
    /// <param name="messageIdBytes">bytes from which it will construct the message ID</param>
    /// <returns>message ID</returns>
    private static ushort GetMessageIdFromBytes(byte[] messageIdBytes)
    {
        if (BitConverter.IsLittleEndian)
        {
            Array.Reverse(messageIdBytes);
        }
        return BitConverter.ToUInt16(messageIdBytes, 0);
    }
   
    /// <summary>
    /// Converts bytes to string and writes end index to endIndex
    /// </summary>
    /// <param name="bytes">array of bytes</param>
    /// <param name="startIndex">start index from which it will be converted</param>
    /// <param name="endIndex">end index of the message component</param>
    /// <returns></returns>
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
    