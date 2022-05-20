using Nito.AsyncEx;
using SayDB.IoJobs;
using System.Collections.Concurrent;

namespace SayDB;

internal class FileManager
{
    public string DataFolderPath { get; }

    private readonly ConcurrentDictionary<object, int> _ticketMap = new();

    private readonly object _maxTicketLock = new object();
    private int _maxTicket;

    private readonly AsyncLock _actionLock = new();

    public FileManager(string dataFolderPath) => DataFolderPath = dataFolderPath;

    public object[] PrimaryKeys => _ticketMap.Keys.ToArray();


    public async Task RecoverFileTicketsAsync<D, K>(DbCollection<D, K> collection)
    {
        await Directory.GetFiles(DataFolderPath).ForEachAsync(async file =>
        {
            try
            {
                var bytes = await LoadBytesFromFileAsync(file);

                var ctx = new IoJobContext(collection) { LoadClassProperties = false };

                var obj = collection.CreateEmptyObject();
                await new BytesConverter(ctx).FillObjectPropValuesAsync(bytes, obj);

                var fileTicket = int.Parse(Path.GetFileName(file));
                var key = collection.GetPrimaryKey(obj);

                RegisterTicket(key, fileTicket);
            }
            catch { }
        });
    }

    internal void RegisterTicket(object key, int fileTicket)
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

    public int GetTicketSafe(object key)
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

    internal bool FileExists(object primaryKey) => _ticketMap.ContainsKey(primaryKey);

    public string GetDataFilePath(int fileTicket)
    {
        return Path.Combine(DataFolderPath, fileTicket.ToString());
    }

    public async Task<byte[]> LoadBytesAsync(object primaryKey)
    {
        using(await _actionLock.LockAsync())
        {
            var fileTicket = GetFileTicket(primaryKey);
            var dataFilePath = GetDataFilePath(fileTicket);
            return await LoadBytesFromFileAsync(dataFilePath);
        }
    }

    private static async Task<byte[]> LoadBytesFromFileAsync(string dataFilePath)
        => await File.ReadAllBytesAsync(dataFilePath);

    public async Task SaveFileAsync(object primaryKey, byte[] bytes)
    {
        using (await _actionLock.LockAsync())
        {
            var fileTicket = GetOrCreateFileTicket(primaryKey);

            var dataFilePath = GetDataFilePath(fileTicket);

            await File.WriteAllBytesAsync(dataFilePath, bytes);
        }
    }

    internal void DeleteFile(object primaryKey)
    {
        using (_actionLock.Lock())
        {
            var fileTicket = GetFileTicket(primaryKey);

            var path = GetDataFilePath(fileTicket);

            File.Delete(path);

            _ticketMap.Remove(primaryKey, out _);
        }
    }

    internal void Clear()
    {
        using (_actionLock.Lock())
        {
            _ticketMap.Clear();
            FileUtil.CleanupFolderContents(DataFolderPath);

            lock (_maxTicketLock) _maxTicket = 0;
        }
    }
}
