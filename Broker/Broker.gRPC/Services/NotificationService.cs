using Grpc.Core;

namespace Broker.gRPC.Services;

public class NotificationService : Notifier.NotifierBase
{
    public override Task<NotifyReply> Notify(NotifyRequest request, ServerCallContext context)
    {
        Console.WriteLine($"Received: {request.Content}");
        return Task.FromResult(new NotifyReply
        {
            Success = true
        });
    }
}