/*
 * File: MessageType.cs
 * Description: Enum for different message types in IPK24Chat.
 * Author: Michal Balogh, xbalog06
 * Date: 30.3.2024
 */

namespace ChatApp.Enums;

/// <summary>
/// Represents all possible types of messages in IPK24Chat
/// </summary>
public enum MessageType
{
    None = 0,
    Err,
    Confirm,
    Reply,
    NotReply,
    Auth,
    Join,
    Msg,
    Bye,
    MsgOrJoin,
    Invalid
}


