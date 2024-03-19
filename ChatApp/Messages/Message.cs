namespace ChatApp.Messages;

public abstract class Message
{
    public abstract string Craft();
    public abstract void PrintOutput();
    public abstract void CheckReceivedMessage(Message message);
}