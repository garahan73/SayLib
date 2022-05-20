namespace SayDB.Test;

[TestClass]
public class SayDBTest
{
    private const string DB_DATA_FOLDER = "c:/tmp/db2";

    class Data
    {
        public int ID;
        public string Name { get; set; } = "";

        public Sub? Sub { get; set; }
    }

    class Sub
    {
        public int Key;
        public string Title { get; set; } = "";
    }

    class Data2
    {
        public int ID;
        public string Name { get; set; } = "";

        public Data2? Pair { get; set; }
    }


    [TestMethod]
    public async Task DbTest1_0_SingleItemIOAsync()
    {
        var db = new SayDB(DB_DATA_FOLDER);
        var collection = db.CreateCollection<Data, int>(d => d.ID);

        await collection.SaveAsync(new Data { ID = 1, Name = "a" });

        var data = await collection.LoadAsync(1);
        Assert.AreEqual("a", data.Name);
    }

    [TestMethod]
    public async Task DbTest1_1_UpdateAsync()
    {
        var db = new SayDB(DB_DATA_FOLDER);
        var collection = db.CreateCollection<Data, int>(d => d.ID);

        await collection.SaveAsync(new Data { ID = 1, Name = "a" });
        await collection.SaveAsync(new Data { ID = 1, Name = "x" });

        var data = await collection.LoadAsync(1);
        Assert.AreEqual("x", data.Name);
    }

    [TestMethod]
    public async Task DbTest2_0_ItemsIOAsync()
    {
        var db = new SayDB(DB_DATA_FOLDER);
        var collection = db.CreateCollection<Data, int>(d => d.ID);

        await collection.SaveAsync(new Data { ID = 3, Name = "a" });
        await collection.SaveAsync(new Data { ID = 4, Name = "b" });

        var data = await collection.LoadAsync(3);
        Assert.AreEqual("a", data.Name);

        data = await collection.LoadAsync(4);
        Assert.AreEqual("b", data.Name);
    }

    [TestMethod]
    public async Task DbTest2_1_LoadAllAsync()
    {
        var db = new SayDB(DB_DATA_FOLDER);
        var collection = db.CreateCollection<Data, int>(d => d.ID);
        collection.Clear();

        await collection.SaveAsync(new Data { ID = 3, Name = "a" });
        await collection.SaveAsync(new Data { ID = 4, Name = "b" });

        var items = await collection.LoadAllAsync();

        Assert.AreEqual(3, items[0].ID);
        Assert.AreEqual("a", items[0].Name);

        Assert.AreEqual(4, items[1].ID);
        Assert.AreEqual("b", items[1].Name);
    }

    [TestMethod]
    public async Task DbTest3_PersitencyTestAsync()
    {
        var db = new SayDB(DB_DATA_FOLDER);
        var collection = db.CreateCollection<Data, int>(d => d.ID);

        await collection.SaveAsync(new Data { ID = 100, Name = "x" });

        db = new SayDB(DB_DATA_FOLDER);
        collection = db.CreateCollection<Data, int>(d => d.ID);

        var data = await collection.LoadAsync(100);
        Assert.AreEqual("x", data.Name);
    }

    [TestMethod]
    public async Task DbTest4_PrimaryKeyQueryAsync()
    {
        var db = new SayDB(DB_DATA_FOLDER);
        var collection = db.CreateCollection<Data, int>(d => d.ID);

        await collection.SaveAsync(new Data { ID = 3, Name = "a" });
        await collection.SaveAsync(new Data { ID = 4, Name = "b" });

        var dataList = from key in collection.PrimaryKeys
            where (int)key == 3
            select collection.Load(key);

        var array = dataList.ToArray();

        Assert.AreEqual(1, array.Length);
        Assert.AreEqual("a", array[0].Name);
    }

    [TestMethod]
    public async Task DbTest5_0_SubItemAutoSaveAsync()
    {
        var db = new SayDB(DB_DATA_FOLDER);
        var mainCollection = db.CreateCollection<Data, int>(d => d.ID);
        var subCollection = db.CreateCollection<Sub, int>(d => d.Key);

        await mainCollection.SaveAsync(new Data { ID = 3, Name = "a", Sub = new Sub { Key = 1, Title = "s1" } });

        var sub = await subCollection.LoadAsync(1);
        
        Assert.AreEqual("s1", sub.Title);
    }

    [TestMethod]
    public async Task DbTest5_1_NonDbSubItemAutoSaveAsync()
    {
        var db = new SayDB(DB_DATA_FOLDER);
        var mainCollection = db.CreateCollection<Data, int>(d => d.ID);

        await mainCollection.SaveAsync(new Data { ID = 3, Name = "a", Sub = new Sub { Key = 1, Title = "s1" } });

        var item = await mainCollection.LoadAsync(3);

        Assert.AreEqual("a", item.Name);
        Assert.AreEqual(1, item.Sub?.Key);
        Assert.AreEqual("s1", item.Sub?.Title);
    }

    [TestMethod]
    public async Task DbTest6_SubItemAutoLoadAsync()
    {
        var db = new SayDB(DB_DATA_FOLDER);
        var mainCollection = db.CreateCollection<Data, int>(d => d.ID);
        var subCollection = db.CreateCollection<Sub, int>(d => d.Key);

        await mainCollection.SaveAsync(new Data { ID = 3, Name = "a", Sub = new Sub { Key = 1, Title = "s1" } });

        var item = await mainCollection.LoadAsync(3);
        var sub = item.Sub ?? throw new NullReferenceException("Sub is null");

        Assert.AreEqual("s1", item.Sub.Title);
    }

    [TestMethod]
    public async Task DbTest7_ResolvingMutualReferenceAsync()
    {
        var db = new SayDB(DB_DATA_FOLDER);
        var mainCollection = db.CreateCollection<Data2, int>(d => d.ID);
        mainCollection.Clear();

        var item1 = new Data2 { ID = 1, Name = "a" };
        var item2 = new Data2 { ID = 2, Name = "b" };

        // set mutual reference
        item1.Pair = item2;
        item2.Pair = item1;

        await mainCollection.SaveAsync(item1);

        var item = await mainCollection.LoadAsync(1);

        Assert.AreEqual("a", item.Name);
        Assert.AreEqual(2, item.Pair?.ID);
        Assert.AreEqual("b", item.Pair?.Name);
    }

}