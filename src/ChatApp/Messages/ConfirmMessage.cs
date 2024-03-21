using ChatApp.Enums;

namespace ChatApp.Messages;

public class ConfirmMessage(ushort messageId = 0) : Message
{
    public override MessageType Type => MessageType.Confirm;
    public override string? CraftTcp()
    {
        return null;
    }
    
    public override byte[]? CraftUdp()
    {
        byte[] messageTypeBytes = [(byte)MessageTypeByte.Confirm];
        byte[] messageIdBytes = GetMessageId(messageId);

        byte[] messageBytes = ByteMessageConcat(messageTypeBytes, messageIdBytes);

        return messageBytes;
    }



    public override void PrintOutput()
    {
        return;
    }
}