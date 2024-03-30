/*
 * File: ConfirmMessage.cs
 * Description: Message class for CONFIRM message
 * Author: Michal Balogh, xbalog06
 * Date: 30.03.2024
 */

using ChatApp.Enums;

namespace ChatApp.Messages;

public class ConfirmMessage(ushort messageId = 0) : Message(messageId)
{
    public override MessageType Type => MessageType.Confirm;
    public override string? CraftTcp()
    {
        return null;
    }
    
    public override byte[]? CraftUdp()
    {
        byte[] messageTypeBytes = [(byte)MessageTypeByte.Confirm];
        byte[] messageIdBytes = GetMessageId(MessageId);

        return ByteMessageConcat(messageTypeBytes, messageIdBytes);
    }



    public override void PrintOutput()
    {
        return;
    }
}