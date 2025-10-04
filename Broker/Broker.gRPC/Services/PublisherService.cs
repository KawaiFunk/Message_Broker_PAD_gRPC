using Broker.gRPC.Models;
using Broker.gRPC.Services.Interfaces;
using Grpc.Core;

namespace Broker.gRPC.Services;

public class PublisherService(
    IMessageStorageService messageStorageService)
    : Publisher.PublisherBase
{
    public override Task<PublishReply> PublishMessage(PublishRequest request, ServerCallContext context)
    {
        var message = new Message(request.Topic, request.Content);
        messageStorageService.Add(message);

        return Task.FromResult(new PublishReply
        {
            Success = true,
        });
    }
}