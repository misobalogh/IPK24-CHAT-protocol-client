using System.Text;
using ChatApp.Enums;

namespace ChatApp.Messages;

public class AuthMessage(string username, string displayName, string secret, ushort messageId = 0) : Message
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
        byte[] messageIdBytes = GetMessageId(messageId);

        byte[] messageBytes = ByteMessageConcat(authMessageTypeBytes, messageIdBytes, usernameBytes, NullTerminator, displayNameBytes, NullTerminator, secretBytes, NullTerminator);

        return messageBytes;
    }

    
    public override void PrintOutput()
    {
        return;
    }
}