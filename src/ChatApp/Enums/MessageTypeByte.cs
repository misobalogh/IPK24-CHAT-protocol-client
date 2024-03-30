/*
 * File: MessageTypeByte.cs
 * Description: Enum for different messages for UDP variant.
 * Author: Michal Balogh, xbalog06
 * Date: 30.3.2024
 */

namespace ChatApp.Enums;

/// <summary>
/// Represents the types of messages in UDP protocol
/// </summary>
public enum MessageTypeByte : byte
{
    Confirm = 0x00,
    Reply = 0x01,
    Auth = 0x02,
    Join = 0x03,
    Msg = 0x04,
    Err = 0xFE,
    Bye = 0xFF
}