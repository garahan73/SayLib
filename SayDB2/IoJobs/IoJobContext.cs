namespace SayDB2.IoJobs;

record IoJobContext(DbCollection Collection, AlreadyHandledItems AlreadyHandledItems)
{
    public bool LoadClassProperties { get; set; } = true;

    public IoJobContext(DbCollection Collection) : this(Collection, new())
    {

    }

    internal IoJobContext Copy(DbCollection collection) => new IoJobContext(collection, AlreadyHandledItems)
    {
        LoadClassProperties = LoadClassProperties
    };
}

record IoJobContextOwner(IoJobContext JobContext)
{
    public DbCollection Collection => JobContext.Collection;

    protected DbContext DbContext => JobContext.Collection.DbContext;

    public AlreadyHandledItems AlreadyHandledItems => JobContext.AlreadyHandledItems;
}

