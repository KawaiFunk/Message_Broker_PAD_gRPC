using Broker.gRPC.Constants;
using Broker.gRPC.Services;
using Broker.gRPC.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseUrls($"{Constants.Host}:{Constants.PublisherPort}");

builder.Services.AddGrpc();
builder.Services.AddSingleton<IMessageStorageService, MessageStorageService>();
builder.Services.AddSingleton<IConnectionStorageService, ConnectionStorageService>();
builder.Services.AddHostedService<SenderWorker>();

var app = builder.Build();

app.MapGrpcService<PublisherService>();
app.MapGrpcService<SubscriberService>();
app.MapGrpcService<NotificationService>();

app.MapGet("/",
    () => "Communication with gRPC endpoints must be made through a gRPC client. " +
          "See: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();