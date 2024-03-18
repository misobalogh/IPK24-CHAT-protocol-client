namespace ChatApp.Messages;

public class AuthMessage(string username, string displayName, string secret) : Message
{
    public override string Craft()
    {
        return $"AUTH {username} AS {displayName} USING {secret}\r\n";
    }

    public override void CheckReceivedMessage(Message message)
    {
        throw new NotImplementedException();
    }
}