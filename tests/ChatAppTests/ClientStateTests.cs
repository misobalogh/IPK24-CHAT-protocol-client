using ChatApp;
using Xunit;
using ChatApp.Enums;

namespace ChatAppTests
{
    public class ClientStateTests
    {
        [Theory]
        [InlineData(MessageType.None, State.Auth, MessageType.Auth)]
        [InlineData(MessageType.Reply, State.Error, MessageType.Err)]
        [InlineData(MessageType.NotReply, State.Error, MessageType.Err)]
        [InlineData(MessageType.Err, State.Error, MessageType.Err)]
        [InlineData(MessageType.Msg, State.Error, MessageType.Err)]
        [InlineData(MessageType.Join, State.Error, MessageType.Err)]
        [InlineData(MessageType.MsgOrJoin, State.Error, MessageType.Err)]
        [InlineData(MessageType.Invalid, State.Error, MessageType.Err)]
        [InlineData(MessageType.Auth, State.Error, MessageType.Err)]
        public void NextState_FromInitialState_ShouldTransitionToExpectedStateAndOutput(MessageType input, State expectedState, MessageType expectedOutput)
        {
            var clientState = new ClientState();

            var nextState = clientState.NextState(input, out var output);

            Assert.Equal(expectedState, nextState);
            Assert.Equal(expectedOutput, output);
        }
        
        [Theory]
        [InlineData(MessageType.None, State.End, MessageType.Bye)]
        [InlineData(MessageType.Err, State.End, MessageType.Bye)]
        [InlineData(MessageType.NotReply, State.Auth, MessageType.Auth)]
        [InlineData(MessageType.Reply, State.Open, MessageType.MsgOrJoin)]
        [InlineData(MessageType.Msg, State.Error, MessageType.Err)]
        [InlineData(MessageType.Join, State.Error, MessageType.Err)]
        [InlineData(MessageType.Invalid, State.Error, MessageType.Err)]
        [InlineData(MessageType.Auth, State.Error, MessageType.Err)]
        public void NextState_FromAuthState_ShouldTransitionToExpectedStateAndOutput(MessageType input, State expectedState, MessageType expectedOutput)
        {
            var clientState = new ClientState();
            
            // Transition to Auth state
            clientState.NextState(MessageType.None, out _);

            var nextState = clientState.NextState(input, out var output);

            Assert.Equal(expectedState, nextState);
            Assert.Equal(expectedOutput, output);
        }
        
        [Theory]
        [InlineData(MessageType.None, State.Open, MessageType.MsgOrJoin)]
        [InlineData(MessageType.NotReply, State.Open, MessageType.MsgOrJoin)]
        [InlineData(MessageType.Reply, State.Open, MessageType.MsgOrJoin)]
        [InlineData(MessageType.Msg, State.Open, MessageType.MsgOrJoin)]
        [InlineData(MessageType.Err, State.End, MessageType.Bye)]
        [InlineData(MessageType.Bye, State.End, MessageType.None)]
        [InlineData(MessageType.Join, State.Error, MessageType.Err)]
        [InlineData(MessageType.Invalid, State.Error, MessageType.Err)]
        [InlineData(MessageType.Auth, State.Error, MessageType.Err)]
        public void NextState_FromOpenState_ShouldTransitionToExpectedStateAndOutput(MessageType input, State expectedState, MessageType expectedOutput)
        {
            var clientState = new ClientState();
            
            // Transition to Auth state
            clientState.NextState(MessageType.None, out _);
            
            // Transition to Open state
            clientState.NextState(MessageType.Reply, out _);

            var nextState = clientState.NextState(input, out var output);

            Assert.Equal(expectedState, nextState);
            Assert.Equal(expectedOutput, output);
        }
        
        [Theory]
        [InlineData(MessageType.None, State.End, MessageType.Bye)]
        [InlineData(MessageType.Err, State.Error, MessageType.Err)]
        [InlineData(MessageType.NotReply, State.Error, MessageType.Err)]
        [InlineData(MessageType.Reply, State.Error, MessageType.Err)]
        [InlineData(MessageType.Msg, State.Error, MessageType.Err)]
        [InlineData(MessageType.Join, State.Error, MessageType.Err)]
        [InlineData(MessageType.Invalid, State.Error, MessageType.Err)]
        [InlineData(MessageType.Auth, State.Error, MessageType.Err)]
        public void NextState_FromErrorState_ShouldTransitionToExpectedStateAndOutput(MessageType input, State expectedState, MessageType expectedOutput)
        {
            var clientState = new ClientState();
            
            // Transition to Error state
            clientState.NextState(MessageType.Err, out _);

            var nextState = clientState.NextState(input, out var output);

            Assert.Equal(expectedState, nextState);
            Assert.Equal(expectedOutput, output);
        }
        
        [Theory]
        [InlineData(MessageType.None, State.End, MessageType.None)]
        [InlineData(MessageType.Err, State.End, MessageType.None)]
        [InlineData(MessageType.NotReply, State.End, MessageType.None)]
        [InlineData(MessageType.Reply, State.End, MessageType.None)]
        [InlineData(MessageType.Msg, State.End, MessageType.None)]
        [InlineData(MessageType.Bye, State.End, MessageType.None)]
        [InlineData(MessageType.Join, State.End, MessageType.None)]
        [InlineData(MessageType.Invalid, State.End, MessageType.None)]
        [InlineData(MessageType.Auth, State.End, MessageType.None)]
        public void NextState_FromEndState_ShouldStayInEndState(MessageType input, State expectedState, MessageType expectedOutput)
        {
            var clientState = new ClientState();
            
            // Transition to Auth state
            clientState.NextState(MessageType.None, out _);
            
            // Transition to End state
            clientState.NextState(MessageType.Err, out _);
            
            var nextState = clientState.NextState(input, out var output);

            Assert.Equal(expectedState, nextState);
            Assert.Equal(expectedOutput, output);
        }
    }
}