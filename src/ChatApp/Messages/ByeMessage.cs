using ChatApp.Enums;

namespace ChatApp.Messages;

public class ByeMessage(ushort messageId = 0) : Message
{
    public override MessageType Type => MessageType.Bye;
    public override string? CraftTcp()
    {
        return "BYE\r\n";
    }
    
    public override byte[]? CraftUdp()
    {
        byte[] messageTypeBytes = [(byte)MessageTypeByte.Bye];
        byte[] messageIdBytes = GetMessageId(messageId);

        byte[] messageBytes = ByteMessageConcat(messageTypeBytes, messageIdBytes);

        return messageBytes;
    }

    
    public override void PrintOutput()
    {
        return;
    }
}