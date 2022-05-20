namespace SayDB2.IoJobs;



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

        var bytes = await new BytesConverter(Context).ToBytesAsync(data);
        await Collection.FileManager.SaveFileAsync(primaryKey, bytes);
    }
}

record LoadJob(IoJobContext Context): IoJobContextOwner(Context)
{
    public async Task<object> LoadAsync(object primaryKey)
    {
        var type = Collection.ItemType;

        if (AlreadyHandledItems.Contains(type, primaryKey))
        {
            return AlreadyHandledItems.GetObject(type, primaryKey);
        }
        else
        {
            var bytes = await Collection.FileManager.LoadBytesAsync(primaryKey);

            var data = Collection.CreateEmptyObject();

            AlreadyHandledItems.Add(type, primaryKey, data);

            await new BytesConverter(Context).FillObjectPropValuesAsync(bytes, data);

            return data;
        }
    }
}

