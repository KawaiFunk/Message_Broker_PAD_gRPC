using Broker.gRPC.Services.Interfaces;
using Grpc.Core;

namespace Broker.gRPC.Services;

public class SenderWorker : IHostedService
{
    private Timer _timer;
    private const int _interval = 1000;
    private readonly IMessageStorageService _messageStorageService;
    private readonly IConnectionStorageService _connectionStorageService;

    public SenderWorker(IServiceScopeFactory scopeFactory)
    {
        using var scope = scopeFactory.CreateScope();
        _messageStorageService = scope.ServiceProvider.GetRequiredService<IMessageStorageService>();
        _connectionStorageService = scope.ServiceProvider.GetRequiredService<IConnectionStorageService>();
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _timer = new Timer(DoSendWork, null, 0, _interval);
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _timer?.Change(Timeout.Infinite, 0);
        return Task.CompletedTask;
    }

    private void DoSendWork(object state)
    {
        while (!_messageStorageService.IsEmpty())
        {
            var message = _messageStorageService.GetNext();

            if (message == null)
            {
                continue;
            }

            var connections = _connectionStorageService.GetConnectionsByTopic(message.Topic);
            foreach (var connection in connections)
            {
                var client = new Notifier.NotifierClient(connection.Channel);
                var request = new NotifyRequest
                {
                    Content = message.Content
                };

                try
                {
                    var reply = client.Notify(request);
                    Console.WriteLine("[SENDER] Sent message to " + connection.Address + " on topic " + message.Topic);
                    Console.WriteLine("[SENDER] Reply: " + reply.Success);
                }
                catch (RpcException e)
                {
                    if (e.StatusCode == StatusCode.Unavailable)
                    {
                        Console.WriteLine("[SENDER][ERROR] Connection to " + connection.Address +
                                          " is unavailable. Removing connection.");
                    }
                    else
                    {
                        Console.WriteLine("[SENDER][ERROR] Could not send message to " + connection.Address +
                                          " on topic " +
                                          message.Topic);
                        Console.WriteLine("[SENDER][ERROR] " + e.Status.Detail);
                    }

                    _connectionStorageService.Remove(connection.Address);
                }
                catch (Exception e)
                {
                    Console.WriteLine("[SENDER][ERROR] Could not send message to " + connection.Address + " on topic " +
                                      message.Topic);
                    Console.WriteLine("[SENDER][ERROR] " + e.Message);
                }
            }
        }
    }
}