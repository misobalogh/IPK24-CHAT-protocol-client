/*
 * File: InvalidMessage.cs
 * Description: Message dummy class for invalid message
 * Author: Michal Balogh, xbalog06
 * Date: 30.03.2024
 */

using ChatApp.Enums;

namespace ChatApp.Messages;

/// <summary>
/// Dummy class for indication that something went wrong
/// </summary>
public class InvalidMessage(ushort messageId = 0) : Message(messageId)
{
    public override MessageType Type => MessageType.Invalid;
    public override string? CraftTcp()
    {
        return null;
    }
    
    public override byte[]? CraftUdp()
    {
        return null;
    }
    
    public override void PrintOutput()
    {
        return;
    }
}