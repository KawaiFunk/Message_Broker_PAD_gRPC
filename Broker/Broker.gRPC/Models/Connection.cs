using Grpc.Net.Client;

namespace Broker.gRPC.Models;

public class Connection(string address, string topic)
{
    public string Address { get; } = address;
    public string Topic { get; } = topic;
    public GrpcChannel Channel { get; } = GrpcChannel.ForAddress(address);
}