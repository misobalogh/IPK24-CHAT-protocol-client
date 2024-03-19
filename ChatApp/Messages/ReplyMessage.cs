namespace ChatApp.Messages;

public class ReplyMessage(bool isOk, string messageContent) : Message
{
    public override string Craft()
    {
        string status = isOk ? "OK" : "NOK";
        return $"REPLY {status} IS {messageContent}\r\n";
    }

    public override void CheckReceivedMessage(Message message)
    {
        throw new NotImplementedException();
    }
    
    public override void PrintOutput()
    {
        string status = isOk ? "Success" : "Failure";
        Console.Error.WriteLine($"{status}: {messageContent}");
    }
}