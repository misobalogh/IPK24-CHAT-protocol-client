using ChatApp.Enums;

namespace ChatApp.Messages;

public class ReplyMessage(bool isOk, string messageContent) : Message
{
    public override MessageType Type => isOk ? MessageType.Reply : MessageType.NotReply;
    public override string? Craft()
    {
        if (MessageGrammar.IsContent(messageContent))
        {
            string status = isOk ? "OK" : "NOK";
            return $"REPLY {status} IS {messageContent}\r\n";
        }

        ErrorHandler.InternalError("Invalid format of MSG message");
        return null;
       
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