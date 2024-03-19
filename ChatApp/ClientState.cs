using ChatApp.Enums;

namespace ChatApp;

public class ClientState
{
    private State _currentState = State.Start;
    public State NextState(MessageType clientInput, MessageType clientOutput)
    {
        switch (_currentState)
        {
            case State.Start:
                if (clientInput == MessageType.None && clientOutput == MessageType.Auth)
                {
                    _currentState = State.Auth;
                }
                else
                {
                    _currentState = State.Error;
                }
                break;
            
            case State.Auth:
                if (clientInput is MessageType.NotReply && clientOutput == MessageType.Auth)
                {
                    _currentState = State.Auth;
                }

                else if (clientInput == MessageType.Reply && clientOutput == MessageType.None)
                {
                    _currentState = State.Open;
                    
                }

                else if (clientInput is MessageType.Err or MessageType.None && clientOutput == MessageType.Bye)
                {
                    _currentState = State.End;
                }

                else
                {
                    _currentState = State.Error;
                }
                
                break;
            
            case State.Open:
                if (clientInput is MessageType.Msg or MessageType.Reply or MessageType.NotReply && clientOutput == MessageType.None)
                {
                    _currentState = State.Open;
                }
                
                else if (clientInput == MessageType.None && clientOutput is MessageType.Join or MessageType.Msg)
                {
                    _currentState = State.Open;
                }
                
                else if ((clientInput == MessageType.Err && clientOutput == MessageType.Bye)
                         || (clientInput == MessageType.Bye && clientOutput == MessageType.None))
                {
                    _currentState = State.End;
                }

                else
                {
                    _currentState = State.Error;
                }
                
                break;
            
            case State.Error:
                if (clientInput == MessageType.None && clientOutput == MessageType.Bye)
                {
                    _currentState = State.End;
                }
                else
                {
                    _currentState = State.Error;
                }
                break;
            
            case State.End:
                if (clientOutput == MessageType.None)
                {
                    _currentState = State.End;
                }
                else
                {
                    _currentState = State.Error;
                }
                break;
            
            default:
                ErrorHandler.ExitWith($"Invalid message type {clientInput} or {clientOutput}.", ExitCode.UnknownMessageType);
                break;
        }

        return _currentState;
    }

    public State GetCurrentState()
    {
        return _currentState;
    }
}


