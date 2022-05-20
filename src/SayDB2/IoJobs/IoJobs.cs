namespace SayDB.IoJobs;



record SaveJob(IoJobContext Context): IoJobContextOwner(Context)
{
    public async Task SaveAsync(object data)
    {
        var type = data.GetType() ?? Collection.ItemType;
        var primaryKey = Collection.GetPrimaryKey(data);

        if (AlreadyHandledItems.Contains(type, primaryKey))
            return;
        else
            AlreadyHandledItems.Add(type, primaryKey, data);

        using var stream = await new PropertiesJob(Context).ObjectToStreamAsync(data);

        await Collection.FileManager.SaveFileAsync(primaryKey, stream);
    }
}

record LoadJob(IoJobContext Context): IoJobContextOwner(Context)
{
    public async Task<object> LoadAsync(object primaryKey)
    {
        var type = Collection.ItemType;

        if (AlreadyHandledItems.Contains(type, primaryKey))
        {
            return AlreadyHandledItems.GetItemObject(type, primaryKey);
        }
        else
        {
            using var stream = await Collection.FileManager.LoadStreamAsync(primaryKey);

            var data = Collection.CreateEmptyObject();

            AlreadyHandledItems.Add(type, primaryKey, data);

            await new PropertiesJob(Context).StreamToObjectAsync(stream, data);

            return data;
        }
    }
}

