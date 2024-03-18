namespace ChatApp.MessageParameters;

using ChatApp;
public class MessageId(ushort id)
{
    public ushort Value => id;

    public static MessageId Parse(string userInput)
    {
        if (!ushort.TryParse(userInput, out ushort messageId))
        {
            ErrorHandler.InformUser("Wrong messageId");
        }

        return new MessageId(messageId);
    }
}