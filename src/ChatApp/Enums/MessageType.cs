namespace ChatApp.Enums;

public enum MessageType
{
    None = 0,
    Err,
    Confirm,
    Reply,
    NotReply,
    Auth,
    Join,
    Msg,
    Bye,
    MsgOrJoin
}