namespace ChatApp.Messages;

public class JoinMessage(string channelId, string displayName) : Message
{
    public override string Craft()
    {
        return $"JOIN {channelId} AS {displayName}\r\n";
    }

    public override void CheckReceivedMessage(Message message)
    {
        throw new NotImplementedException();
    }
}