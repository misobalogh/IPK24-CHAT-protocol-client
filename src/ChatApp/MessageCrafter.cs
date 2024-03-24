using ChatApp.Enums;
using ChatApp.Messages;

namespace  ChatApp;

public class MessageCrafter(ProtocolVariant protocolVariant)
{
    public object? Craft(Message message)
    {
        if (protocolVariant == ProtocolVariant.Tcp)
        {
            return message.CraftTcp();
        }
        else
        {
            return message.CraftUdp();
        }
    }
}
