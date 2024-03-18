namespace ChatApp.Messages;

public class ByeMessage : Message
{
    public override string Craft()
    {
        return "BYE\r\n";
    }

    public override void CheckReceivedMessage(Message message)
    {
        throw new NotImplementedException();
    }
    
    public override string Output()
    {
        throw new NotImplementedException();
    }
}