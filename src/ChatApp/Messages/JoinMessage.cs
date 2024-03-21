using System.Text;
using ChatApp.Enums;

namespace ChatApp.Messages;

public class JoinMessage(string channelId, string displayName, ushort messageId = 0) : Message
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

        byte[] messageTypeBytes = new byte[] { (byte)MessageTypeByte.Join };
        byte[] messageIdBytes = GetMessageId(messageId);
        byte[] channelIdBytes = Encoding.ASCII.GetBytes(channelId);
        byte[] displayNameBytes = Encoding.ASCII.GetBytes(displayName);

        byte[] messageBytes = ByteMessageConcat(messageTypeBytes, messageIdBytes, channelIdBytes, NullTerminator, displayNameBytes, NullTerminator);

        return messageBytes;
    }

    
    public override void PrintOutput()
    {
        return;
    }
}