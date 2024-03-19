namespace ChatApp.Messages;

public class ErrMessage(string displayName, string messageContent) : Message
{
    public override string Craft()
    {
        return $"ERR FROM {displayName} IS {messageContent}\r\n";
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