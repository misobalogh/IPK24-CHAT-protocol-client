using ChatApp.Enums;

namespace ChatApp.Messages;

public class ByeMessage : Message
{
    public override MessageType Type => MessageType.Bye;
    public override string? Craft()
    {
        return "BYE\r\n";
    }

    public override void CheckReceivedMessage(Message message)
    {
        throw new NotImplementedException();
    }
    
    public override void PrintOutput()
    {
        return;
    }
}