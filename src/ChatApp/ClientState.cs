using ChatApp.Enums;

namespace ChatApp;

public class ClientState
{
    private State _currentState = State.Start;
    public State NextState(MessageType clientInput, out MessageType clientOutput)
    {
        clientOutput = MessageType.None;
        switch (_currentState)
        {
            case State.Start:
                if (clientInput == MessageType.None)
                {
                    _currentState = State.Auth;
                    clientOutput = MessageType.Auth;
                }
                else
                {
                    _currentState = State.Error;
                    clientOutput = MessageType.Err;
                }
                break;
            
            case State.Auth:
                if (clientInput is MessageType.NotReply)
                {
                    _currentState = State.Auth;
                    clientOutput = MessageType.Auth;
                }

                else if (clientInput == MessageType.Reply)
                {
                    _currentState = State.Open;
                    clientOutput = MessageType.MsgOrJoin;

                }

                else if (clientInput is MessageType.Err or MessageType.None)
                {
                    _currentState = State.End;
                    clientOutput = MessageType.Bye;
                }

                else
                {
                    _currentState = State.Error;
                    clientOutput = MessageType.Err;
                }
                
                break;
            
            case State.Open:
                if (clientInput is MessageType.Msg or MessageType.Reply or MessageType.NotReply)
                {
                    _currentState = State.Open;
                    clientOutput = MessageType.MsgOrJoin;
                }
                
                else if (clientInput == MessageType.None)
                {
                    _currentState = State.Open;
                    clientOutput = MessageType.MsgOrJoin;
                }
                else if (clientInput == MessageType.Err)
                {
                    _currentState = State.End;
                    clientOutput = MessageType.Bye;
                }
                else if (clientInput == MessageType.Bye)
                {
                    _currentState = State.End;
                    clientOutput = MessageType.None;
                }
                else
                {
                    _currentState = State.Error;
                    clientOutput = MessageType.Err;
                }
                
                break;
            
            case State.Error:
                if (clientInput == MessageType.None)
                {
                    _currentState = State.End;
                    clientOutput = MessageType.Bye;
                }
                else
                {
                    _currentState = State.Error;
                    clientOutput = MessageType.Err;
                }
                break;
            
            case State.End:
                clientOutput = MessageType.None;
                _currentState = State.End;
                break;
            
            default:
                ErrorHandler.ExitWith($"Invalid message type {clientInput}.", ExitCode.UnknownMessageType);
                break;
        }
        
        return _currentState;
    }

    public State GetCurrentState()
    {
        return _currentState;
    }
}


