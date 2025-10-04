namespace Broker.gRPC.Models;

public class Message(string topic, string content)
{
    public string Topic { get; } = topic;
    public string Content { get; } = content;
}