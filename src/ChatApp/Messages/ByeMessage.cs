/*
 * File: ByeMessage.cs
 * Description: Message class for BYE message
 * Author: Michal Balogh, xbalog06
 * Date: 30.03.2024
 */

using ChatApp.Enums;

namespace ChatApp.Messages;

public class ByeMessage(ushort messageId = 0) : Message(messageId)
{
    public override MessageType Type => MessageType.Bye;
    public override string? CraftTcp()
    {
        return "BYE\r\n";
    }
    
    public override byte[]? CraftUdp()
    {
        byte[] messageTypeBytes = [(byte)MessageTypeByte.Bye];
        byte[] messageIdBytes = GetMessageId(MessageId);

        return ByteMessageConcat(messageTypeBytes, messageIdBytes);
    }

    
    public override void PrintOutput()
    {
        return;
    }
}