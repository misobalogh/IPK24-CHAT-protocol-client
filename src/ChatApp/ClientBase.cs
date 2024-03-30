/*
 * File: ClientBase.cs
 * Description: Base class for individual variants client implementations.
 * Author: Michal Balogh, xbalog06
 * Date: 30.03.2024
 */

using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;
using ChatApp.Messages;

namespace ChatApp;

/// <summary>
/// Base abstract class for individual variant client implementations.
/// </summary>
public abstract class ClientBase
{
    /// <summary>
    /// Asynchronously sends a message to the server.
    /// </summary>
    /// <param name="message">The message to send.</param>
    public abstract Task SendMessageAsync(Message message);
    
    /// <summary>
    /// Asynchronously receives a message from the server.
    /// </summary>
    public abstract Task<Message?> ReceiveMessageAsync();
    
    /// <summary>
    /// Closes the client connection.
    /// </summary>
    public abstract void Close();
}