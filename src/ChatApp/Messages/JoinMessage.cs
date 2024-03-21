using ChatApp.Enums;

namespace ChatApp.Messages;

public class JoinMessage(string channelId, string displayName) : Message
{
    public override MessageType Type => MessageType.Join;
    public override string? Craft()
    {
        if (MessageGrammar.IsId(channelId)
            && MessageGrammar.IsDname(displayName))
        {
            return $"JOIN {channelId} AS {displayName}\r\n";
        }
        
        ErrorHandler.InternalError("Invalid format of JOIN message");
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