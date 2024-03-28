using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;
using ChatApp.Messages;

namespace ChatApp;
public abstract class ClientBase
{
    protected Socket Socket = null!;
    protected NetworkStream Stream = null!;
    protected StreamWriter Writer = null!;
    protected StreamReader Reader = null!;
    public abstract Task<Message?> ReceiveMessageAsync();
    public abstract Task SendMessageAsync(Message message);
    public abstract void Close();
}