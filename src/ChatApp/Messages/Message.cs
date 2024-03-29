using System.Text;
using ChatApp.Enums;

namespace ChatApp.Messages;

public abstract class Message(ushort messageId = 0)
{
    public abstract MessageType Type { get; }
    public ushort MessageId { get; } = messageId;

    protected readonly byte[] NullTerminator = [0];
    
    // creates message for TCP protocol in correct format
    public abstract string? CraftTcp();
    
    // creates message for UDP protocol in correct format
    public abstract byte[]? CraftUdp();
    
    // Method that prints useful information to client output, if its required from the specific message type
    public abstract void PrintOutput();
    
    // Method used by CraftUdp method, to concatenate bytes to form the final message
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

    protected static byte[] GetMessageId(int messageId)
    {
        var messageIdBytes = new byte[2];
        messageIdBytes[0] = (byte)(messageId >> 8);
        messageIdBytes[1] = (byte)messageId;
        return messageIdBytes;
    }

    protected static byte[] GetBytesFromString(string stringToConvert)
    {
        return Encoding.ASCII.GetBytes(stringToConvert);
    }
}