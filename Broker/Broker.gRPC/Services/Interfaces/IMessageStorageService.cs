using Broker.gRPC.Models;

namespace Broker.gRPC.Services.Interfaces;

public interface IMessageStorageService
{
    void Add(Message message);
    Message GetNext();
    bool IsEmpty();
}