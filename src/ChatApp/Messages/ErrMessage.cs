using System.Text;
using ChatApp.Enums;

namespace ChatApp.Messages;

public class ErrMessage(string displayName, string messageContent, ushort messageId = 0) : Message
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
        byte[] messageIdBytes = GetMessageId(messageId);
        byte[] displayNameBytes = Encoding.ASCII.GetBytes(displayName);
        byte[] messageContentBytes = Encoding.ASCII.GetBytes(messageContent);

        byte[] messageBytes = ByteMessageConcat(messageTypeBytes, messageIdBytes, displayNameBytes, NullTerminator, messageContentBytes, NullTerminator);

        return messageBytes;
    }




    public override void PrintOutput()
    {
        Console.Error.WriteLine($"ERR FROM {displayName}: {messageContent}");
    }
}