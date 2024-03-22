using System.Text;
using ChatApp.Enums;

namespace ChatApp.Messages;

public class ErrMessage(string displayName, string messageContent, ushort messageId = 0) : Message(messageId)
{
    public override MessageType Type => MessageType.Err;
    public override string? CraftTcp()
    {
        if (MessageGrammar.IsDname(displayName)
            && MessageGrammar.IsContent(messageContent))
        {
            return $"ERR FROM {displayName} IS {messageContent}\r\n";
        }

        ErrorHandler.InternalError("Invalid format of ERR message");
        return null;
    }
    
    public override byte[]? CraftUdp()
    {
        if (!MessageGrammar.IsDname(displayName) || !MessageGrammar.IsContent(messageContent))
        {
            ErrorHandler.InternalError("Invalid format of ERR message");
            return null;
        }

        byte[] messageTypeBytes = [(byte)MessageTypeByte.Err];
        byte[] messageIdBytes = GetMessageId(MessageId);
        byte[] displayNameBytes = GetBytesFromString(displayName);
        byte[] messageContentBytes = GetBytesFromString(messageContent);

        return ByteMessageConcat(messageTypeBytes, messageIdBytes, displayNameBytes, NullTerminator, messageContentBytes, NullTerminator);
    }




    public override void PrintOutput()
    {
        Console.Error.WriteLine($"ERR FROM {displayName}: {messageContent}");
    }
}