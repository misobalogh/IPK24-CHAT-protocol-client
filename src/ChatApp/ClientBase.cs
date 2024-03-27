using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace ChatApp;
public abstract class ClientBase
{
    protected Socket Socket = null!;
    protected NetworkStream Stream = null!;
    protected StreamWriter Writer = null!;
    protected StreamReader Reader = null!;
    public abstract Task<object?> ReceiveMessageAsync();
    public abstract Task SendMessageAsync(object? message); 
    public void Close()
    {
        Reader?.Close();
        Writer?.Close();
        Stream?.Close();
        Socket?.Close();
    }
}