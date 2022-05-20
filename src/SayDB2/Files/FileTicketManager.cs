using System.Collections.Concurrent;

namespace SayDB.Files;
internal class FileTicketManager
{
    private readonly ConcurrentDictionary<object, int> _ticketMap = new();

    private readonly object _maxTicketLock = new object();
    private int _maxTicket;

    public object[] PrimaryKeys => _ticketMap.Keys.ToArray();

    public bool FileExists(object primaryKey) => _ticketMap.ContainsKey(primaryKey);

    public void RegisterTicket(object key, int fileTicket)
    {
        _ticketMap.TryAdd(key, fileTicket);

        lock (_maxTicketLock)
        {
            if (fileTicket > _maxTicket)
                _maxTicket = fileTicket;
        }
    }

    public int GetFileTicket(object key)
    {
        if (!_ticketMap.ContainsKey(key))
            throw new Exception($"No ticket issued for key '{key}'");

        return _ticketMap[key];
    }

    private int GetTicketSafe(object key)
    {
        if (!_ticketMap.ContainsKey(key))
            return -1;

        return _ticketMap[key];
    }



    public int GetOrCreateFileTicket(object key)
    {
        if (!_ticketMap.ContainsKey(key))
            return GetNewTicket(key);

        return _ticketMap[key];

    }

    private int GetNewTicket(object key)
    {
        if (_ticketMap.ContainsKey(key))
            throw new Exception($"Ticket already issued for key '{key}'");

        lock (_maxTicketLock)
        {
            _ticketMap.TryAdd(key, ++_maxTicket);

            return _maxTicket;
        }
    }

    internal void RemoveTicket(object primaryKey) => _ticketMap.Remove(primaryKey, out _);
    internal void Clear()
    {
        _ticketMap.Clear();

        lock (_maxTicketLock) _maxTicket = 0;
    }
}
