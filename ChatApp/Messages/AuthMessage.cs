namespace ChatApp.Messages;

public class AuthMessage(string username, string displayName, string secret) : Message
{
    public override string? Craft()
    {
        if (MessageGrammar.IsId(username)
            && MessageGrammar.IsDname(displayName)
            && MessageGrammar.IsSecret(secret))
        {
            return $"AUTH {username} AS {displayName} USING {secret}\r\n";
        }

        ErrorHandler.InternalError("Invalid format of AUTH message");
        return null;
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