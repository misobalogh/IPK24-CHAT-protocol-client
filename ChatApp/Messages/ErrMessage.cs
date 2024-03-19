namespace ChatApp.Messages;

public class ErrMessage(string displayName, string messageContent) : Message
{
    public override string? Craft()
    {
        if (MessageGrammar.IsDname(displayName)
            && MessageGrammar.IsContent(messageContent))
        {
            return $"ERR FROM {displayName} IS {messageContent}\r\n";
        }

        ErrorHandler.InternalError("Invalid format of ERR message");
        return null;
    }

    public override void CheckReceivedMessage(Message message)
    {
        throw new NotImplementedException();
    }
    
    public override void PrintOutput()
    {
        Console.Error.WriteLine($"ERR FROM {displayName}: {messageContent}");
    }
}