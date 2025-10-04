using System.Collections.Concurrent;
using Broker.gRPC.Models;
using Broker.gRPC.Services.Interfaces;

namespace Broker.gRPC.Services;

public class MessageStorageService : IMessageStorageService
{
    private readonly ConcurrentQueue<Message> _messages = new();

    public void Add(Message message)
    {
        _messages.Enqueue(message);
    }

    public Message GetNext()
    {
        if (IsEmpty())
        {
            throw new InvalidOperationException("No messages available.");
        }

        if (_messages.TryDequeue(out var message))
        {
            return message;
        }

        throw new InvalidOperationException("Failed to dequeue message.");
    }

    public bool IsEmpty()
    {
        return _messages.IsEmpty;
    }
}