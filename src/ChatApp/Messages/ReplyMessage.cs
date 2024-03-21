using System.Text;
using ChatApp.Enums;

namespace ChatApp.Messages;

public class ReplyMessage(bool isOk, string messageContent, ushort messageId = 0, ushort refMessageId = 0) : Message
{
    public override MessageType Type => isOk ? MessageType.Reply : MessageType.NotReply;
    public override string? CraftTcp()
    {
        if (MessageGrammar.IsContent(messageContent))
        {
            string status = isOk ? "OK" : "NOK";
            return $"REPLY {status} IS {messageContent}\r\n";
        }

        ErrorHandler.InternalError("Invalid format of MSG message");
        return null;
       
    }
    
    public override byte[]? CraftUdp()
    {
        if (!MessageGrammar.IsContent(messageContent))
        {
            ErrorHandler.InternalError("Invalid format of REPLY message");
            return null;
        }

        byte[] messageContentBytes = GetBytesFromString(messageContent);
        byte[] messageTypeBytes = [(byte)MessageTypeByte.Reply];
        byte[] messageIdBytes = GetMessageId(messageId);
        byte[] isOkByte = [isOk ? (byte)1 : (byte)0];
        byte[] refMessageIdBytes = GetMessageId(refMessageId);

        byte[] messageBytes = ByteMessageConcat(messageTypeBytes, messageIdBytes, isOkByte, refMessageIdBytes, messageContentBytes, NullTerminator);

        return messageBytes;
    }



    public override void PrintOutput()
    {
        string status = isOk ? "Success" : "Failure";
        Console.Error.WriteLine($"{status}: {messageContent}");
    }
}