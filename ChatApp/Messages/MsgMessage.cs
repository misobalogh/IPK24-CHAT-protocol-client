using ChatApp.Enums;

namespace ChatApp.Messages;

public class MsgMessage(string displayName, string messageContent) : Message
{
    public override MessageType Type => MessageType.Msg;

    public override string? Craft()
    {
        if (MessageGrammar.IsDname(displayName)
            && MessageGrammar.IsContent(messageContent))
        {
            return $"MSG FROM {displayName} IS {messageContent}\r\n";
        }

        ErrorHandler.InternalError("Invalid format of MSG message");
        return null;
    }

    public override void CheckReceivedMessage(Message message)
    {
        throw new NotImplementedException();
    }

    public override void PrintOutput()
    {
        Console.WriteLine($"{displayName}: {messageContent}");
    }
}