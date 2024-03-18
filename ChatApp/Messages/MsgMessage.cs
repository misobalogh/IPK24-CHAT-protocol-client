namespace ChatApp.Messages;

public class MsgMessage(string displayName, string messageContent) : Message
{
    public override string Craft()
    {
        return $"MSG FROM {displayName} IS {messageContent}r\n";
    }

    public override void CheckReceivedMessage(Message message)
    {
        throw new NotImplementedException();
    }
}