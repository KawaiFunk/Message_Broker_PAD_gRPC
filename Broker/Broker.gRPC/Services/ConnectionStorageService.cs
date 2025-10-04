using Broker.gRPC.Models;
using Broker.gRPC.Services.Interfaces;

namespace Broker.gRPC.Services;

public class ConnectionStorageService : IConnectionStorageService
{
    private readonly List<Connection> _connections = [];
    private readonly object _locker = new();

    public void Add(Connection connection)
    {
        lock (_locker)
        {
            _connections.Add(connection);
        }
    }

    public void Remove(string address)
    {
        lock (_locker)
        {
            _connections.RemoveAll(c => c.Address == address);
        }
    }

    public IList<Connection> GetConnectionsByTopic(string topic)
    {
        lock (_locker)
        {
            return _connections.Where(c => c.Topic == topic).ToList();
        }
    }
}