using SayDB.Files;
using SayDB.IoJobs;
using SayDB.Props;

namespace SayDB;

public abstract class DbCollection
{
    internal DbContext DbContext { get; }

    public Type ItemType { get; }
    public Func<object, object> GetPrimaryKey { get; }

    internal FileManager FileManager { get; }

    internal PropInfo[] Properties { get; }

    public object[] PrimaryKeys => FileManager.PrimaryKeys;

    public abstract Type? PrimaryKeyType { get; }

    internal DbCollection(DbContext context, string dataFolderPath, Type classType, Func<object, object> getPrimaryKey)
    {
        DbContext = context;

        ItemType = classType;
        GetPrimaryKey = getPrimaryKey;

        Properties = PropertiesFactory.Create(classType, DbContext);

        FileManager = new FileManager(dataFolderPath);
        FileUtil.EnsureFolderPath(dataFolderPath);
    }

    internal object CreateEmptyObject()
    {
        return Activator.CreateInstance(ItemType)
            ?? throw new NullReferenceException($"Crated object is null");
    }

    public async Task SaveAsync(object data)
    {
        var ctx = new IoJobContext(this);
        await new SaveJob(ctx).SaveAsync(data);
    }

    public async Task<object> LoadAsync(object primaryKey)
    {
        var ctx = new IoJobContext(this);
        return await new LoadJob(ctx).LoadAsync(primaryKey);
    }

    public async Task<object[]> LoadAllAsync()
    {
        return (await PrimaryKeys.SelectAsync(k => LoadAsync(k))).ToArray();
    }


    public bool Delete<DATA_TYPE>(object primaryKey)
    {
        if (!FileManager.FileExists(primaryKey))
            return false;

        FileManager.DeleteFile(primaryKey);

        return true;
    }

    public void Clear()
    {
        FileManager.Clear();
    }
}

public class DbCollection<DATA, PRIMARY_KEY> : DbCollection
{
    private Func<DATA, PRIMARY_KEY> GetTypedPrimaryKey { get; }

    public override Type? PrimaryKeyType => typeof(PRIMARY_KEY);

    internal DbCollection(DbContext context, string dataFolderPath, Func<DATA, PRIMARY_KEY> getPrimaryKey)
        : base(context, dataFolderPath, typeof(DATA), 
            o=>getPrimaryKey((DATA)o) ?? throw new NullReferenceException($"Primary key can't be null"))
    {
        GetTypedPrimaryKey = getPrimaryKey;

        FileManager.RecoverFileTicketsAsync(this).Wait();
    }

    public Task SaveAsync(DATA data) => base.SaveAsync(data);

    public new async Task<DATA> LoadAsync(object primaryKey) => (DATA)await base.LoadAsync(primaryKey);

    public new async Task<DATA[]> LoadAllAsync() => (await base.LoadAllAsync()).Cast<DATA>().ToArray();

    public DATA? Load(object primaryKey)
        => LoadAsync(primaryKey).WaitAndGetResult();
    
}

