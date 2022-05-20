using System.Collections.Concurrent;

namespace SayDB;

public class SayDB
{
    private readonly ConcurrentDictionary<Type, DbCollection> _collections = new();

    public string RootDataFolderPath { get; }

    public SayDB(string dataFolderPath, bool ensureDataFolderExistance = true)
    {
        RootDataFolderPath = dataFolderPath;

        if (ensureDataFolderExistance)
            FileUtil.EnsureFolderPath(RootDataFolderPath);
    }

    public DbCollection<DATA_TYPE, KEY_TYPE> CreateCollection<DATA_TYPE, KEY_TYPE>(Func<DATA_TYPE, KEY_TYPE> getPrimaryKey)
    {
        var typeName = typeof(DATA_TYPE).AssemblyQualifiedName;
        if (typeName == null)
            throw new Exception($"{typeof(DATA_TYPE).Name} is invalid type");

        var dataFolderPath = Path.Combine(RootDataFolderPath, typeName);

        var context = new DbContext(RootDataFolderPath, _collections);

        var collection = new DbCollection<DATA_TYPE, KEY_TYPE>(context, dataFolderPath, getPrimaryKey);

        _collections.TryAdd(typeof(DATA_TYPE), collection);

        return collection;
    }


    private DbCollection GetCollection(Type type)
    {
        if (_collections.ContainsKey(type))
            return _collections[type];
        else
            throw new Exception($"Type {type.FullName} is not registered to DB");
    }




}
