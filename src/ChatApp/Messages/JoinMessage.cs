using System.Text;
using ChatApp.Enums;

namespace ChatApp.Messages;

public class JoinMessage(string channelId, string displayName, ushort messageId = 0) : Message(messageId)
{
    public override MessageType Type => MessageType.Join;
    public override string? CraftTcp()
    {
        if (!MessageGrammar.IsId(channelId)
            || !MessageGrammar.IsDname(displayName))
        {
            ErrorHandler.InternalError("Invalid format of JOIN message");
            return null;
        }

        return $"JOIN {channelId} AS {displayName}\r\n";

    }
    
    public override byte[]? CraftUdp()
    {
        if (!MessageGrammar.IsId(channelId) || !MessageGrammar.IsDname(displayName))
        {
            ErrorHandler.InternalError("Invalid format of JOIN message");
            return null;
        }

        byte[] messageTypeBytes = [(byte)MessageTypeByte.Join];
        byte[] messageIdBytes = GetMessageId(MessageId);
        byte[] channelIdBytes = GetBytesFromString(channelId);
        byte[] displayNameBytes = GetBytesFromString(displayName);

        return ByteMessageConcat(messageTypeBytes, messageIdBytes, channelIdBytes, NullTerminator, displayNameBytes, NullTerminator);
    }

    
    public override void PrintOutput()
    {
        return;
    }
}