/*
 * File: Message.cs
 * Description: Base class for message individual classes
 * Author: Michal Balogh, xbalog06
 * Date: 30.03.2024
 */

using System.Text;
using ChatApp.Enums;

namespace ChatApp.Messages;

/// <summary>
/// Base class for all message types
/// </summary>
/// <param name="messageId"> Id of the message - used only in UDP variant</param>
public abstract class Message(ushort messageId = 0)
{
    public abstract MessageType Type { get; }
    public ushort MessageId { get; } = messageId;

    protected readonly byte[] NullTerminator = [0];
    
    /// <summary>
    /// Creates a message for TCP protocol in the correct format.
    /// </summary>
    /// <returns>A string representing the message that can be send via TCP protocol.</returns>
    public abstract string? CraftTcp();
    
    /// <summary>
    /// Creates a message for UDP protocol in the correct format.
    /// </summary>
    /// <returns>An array of bytes that can be send via UDP protocol.</returns>
    public abstract byte[]? CraftUdp();
    
    /// <summary>
    /// Prints useful information to client output, if required by the specific message type.
    /// </summary>
    public abstract void PrintOutput();
    
    /// <summary>
    /// Method used by CraftUdp method, to concatenate bytes to form the final message
    /// </summary>
    protected static byte[] ByteMessageConcat(params byte[][] byteArrays)
    {
        int totalLength = byteArrays.Sum(arr => arr.Length);

        byte[] concatenatedBytes = new byte[totalLength];

        var currentIndex = 0;
        foreach (var byteArray in byteArrays)
        {
            Array.Copy(byteArray, 0, concatenatedBytes, currentIndex, byteArray.Length);
            currentIndex += byteArray.Length;
        }

        return concatenatedBytes;
    }

    /// <summary>
    /// Converts message id (ushort) to byte array, used in UDP variant
    /// </summary>
    protected static byte[] GetMessageId(int messageId)
    {
        var messageIdBytes = new byte[2];
        messageIdBytes[0] = (byte)(messageId >> 8);
        messageIdBytes[1] = (byte)messageId;
        return messageIdBytes;
    }

    /// <summary>
    /// Converts component of message to byte array, used in UDP variant
    /// </summary>
    /// <param name="stringToConvert">Part of message</param>
    /// <returns>byte array of converted string</returns>
    protected static byte[] GetBytesFromString(string stringToConvert)
    {
        return Encoding.ASCII.GetBytes(stringToConvert);
    }
}