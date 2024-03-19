using ChatApp.Enums;

namespace ChatApp.Messages;

public class ConfirmMessage() : Message
{
    public override MessageType Type => MessageType.Confirm;
    public override string? Craft()
    {
        throw new NotImplementedException();
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