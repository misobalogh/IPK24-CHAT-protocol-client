namespace ChatApp.Messages;

public abstract class Message
{
    public abstract string Craft();
    public abstract void CheckReceivedMessage(Message message);
}