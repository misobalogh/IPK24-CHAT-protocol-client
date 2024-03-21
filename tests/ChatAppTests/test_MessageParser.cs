using ChatApp;
using Xunit;
using ChatApp.Messages;

namespace ChatAppTests;

public class MessageParserTests
{
    [Theory]
    [InlineData("ERR FROM user1 IS Invalid command")]
    [InlineData("ERR FROM user2 is err")]
    [InlineData("ERR FROM u13s][a;.??!sr2 is err")]
    [InlineData("ERR FROM user3 iS Invalid command  1 34)_+DSA?'; as spaces ")]
    public void ParseErrMessage_ValidMessage_ReturnsErrMessage(string message)
    {
        var result = MessageParser.ParseMessage(message);
        Assert.IsType<ErrMessage>(result);
    }
    
    [Theory]
    [InlineData("ERR FROM user1 missing is :(")]
    [InlineData("ERR FROM user1 is.")]
    [InlineData("ERR FROM _123456789_123456789_ iS nok")]
    public void ParseErrMessage_InvalidMessage_ReturnsNull(string message)
    {
        var result = MessageParser.ParseMessage(message);
        Assert.Null(result);
    }

    [Theory]
    [InlineData("MSG FROM user1 is Hello, world!")]
    [InlineData("MSG FROM user2 is hi")]
    [InlineData("MSG FROM user3 is Hello, world as]130318401= 'as=d\as]da131= ")]
    public void ParseMsgMessage_ValidMessage_ReturnsMsgMessage(string message)
    {
        var result = MessageParser.ParseMessage(message);
        Assert.IsType<MsgMessage>(result);
    }

    [Theory]
    [InlineData("REPLY OK IS command executed")]
    [InlineData("REPLY NOK iS command failed")]
    [InlineData("REPLY Ok Is ok")]
    [InlineData("REPLY NoK is problem")]
    public void ParseReplyMessage_ValidMessage_ReturnsReplyMessage(string message)
    {
        var result = MessageParser.ParseMessage(message);
        Assert.IsType<ReplyMessage>(result);
    }

    [Theory]
    [InlineData("AUTH 123 AS user1 USING password123")]
    [InlineData("AUTH 123 AS user2 USING 0")]
    [InlineData("AUTH 123 AS user3 USING secret-password-00-33-44")]
    public void ParseAuthMessage_ValidMessage_ReturnsAuthMessage(string message)
    {
        var result = MessageParser.ParseMessage(message);
        Assert.IsType<AuthMessage>(result);
    }

    [Theory]
    [InlineData("JOIN aAbBcCdDeEf AS user")]
    [InlineData("JOIN 1234850 AS 2")]
    [InlineData("JOIN aBc1a2d3 AS user3")]
    [InlineData("JoIn channel-1 AS user-4")]
    [InlineData("join aBc1a2d3 AS user3")]
    public void ParseJoinMessage_ValidMessage_ReturnsJoinMessage(string message)
    {
        var result = MessageParser.ParseMessage(message);
        Assert.IsType<JoinMessage>(result);
    }

    [Theory]
    [InlineData("BYE")]
    [InlineData("bye")]
    [InlineData("byE")]
    public void ParseByeMessage_ValidMessage_ReturnsByeMessage(string message)
    {
        var result = MessageParser.ParseMessage(message);
        Assert.IsType<ByeMessage>(result);
    }
    
    [Theory]
    [InlineData(" BYE ")]
    [InlineData(" bye")]
    [InlineData("byE ")]
    public void ParseByeMessage_InvalidMessage_ReturnsNull(string message)
    {
        var result = MessageParser.ParseMessage(message);
        Assert.Null(result);
    }

    [Theory]
    [InlineData("CONFIRM")]
    [InlineData("UNKNOWN")]
    [InlineData("")]
    public void ParseMessage_InvalidMessage_ReturnsNull(string message)
    {
        var result = MessageParser.ParseMessage(message);
        Assert.Null(result);
    }
}