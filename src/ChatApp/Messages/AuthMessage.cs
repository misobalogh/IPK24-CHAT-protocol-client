/*
 * File: AuthMessage.cs
 * Description: Message class for AUTH message
 * Author: Michal Balogh, xbalog06
 * Date: 30.03.2024
 */

using System.Text;
using ChatApp.Enums;

namespace ChatApp.Messages;

public class AuthMessage(string username, string displayName, string secret, ushort messageId = 0) : Message(messageId)
{
    public override MessageType Type => MessageType.Auth;

    public override string? CraftTcp()
    {
        if (MessageGrammar.IsId(username)
            && MessageGrammar.IsDname(displayName)
            && MessageGrammar.IsSecret(secret))
        {
            return $"AUTH {username} AS {displayName} USING {secret}\r\n";
        }

        ErrorHandler.InternalError("Invalid format of AUTH message");
        return null;
    }
    
    public override byte[]? CraftUdp()
    {
        if (!MessageGrammar.IsId(username) ||
            !MessageGrammar.IsDname(displayName) ||
            !MessageGrammar.IsSecret(secret))
        {
            ErrorHandler.InternalError("Invalid format of AUTH message");
            return null;
        }
        
        byte[] authMessageTypeBytes = [(byte)MessageTypeByte.Auth];
        byte[] usernameBytes = GetBytesFromString(username);
        byte[] displayNameBytes = GetBytesFromString(displayName);
        byte[] secretBytes = GetBytesFromString(secret);
        byte[] messageIdBytes = GetMessageId(MessageId);

        return ByteMessageConcat(authMessageTypeBytes, messageIdBytes, usernameBytes, NullTerminator, displayNameBytes, NullTerminator, secretBytes, NullTerminator);
    }

    
    public override void PrintOutput()
    {
        return;
    }
}