using Broker.gRPC.Models;
using Broker.gRPC.Services.Interfaces;
using Grpc.Core;

namespace Broker.gRPC.Services;

public class SubscriberService(
    IConnectionStorageService connectionStorageService)
    : Subscriber.SubscriberBase
{
    private readonly IConnectionStorageService _connectionStorageService = connectionStorageService;

    public override Task<SubscribeReply> Subscribe(SubscribeRequest request, ServerCallContext context)
    {
        Console.WriteLine($"[SUBSCRIBER] Subscribed to topic: {request.Topic} at {request.Address}");

        try
        {
            var connection = new Connection(request.Address, request.Topic);
            _connectionStorageService.Add(connection);
        }
        catch (Exception e)
        {
            Console.WriteLine($"[SUBSCRIBER][ERROR] Could not add new connection {request.Address} to topic {request.Topic}");
        }

        return Task.FromResult(new SubscribeReply
        {
            Success = true
        });
    }
}