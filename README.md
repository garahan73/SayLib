# SayDB2 examples
[Get started]


//////////////////////////////////
Basic Save/Load
//////////////////////////////////

// create DB with data directory location

        var db = new SayDB("c:/tmp/db");


// register collection with data type and get primary key function

        var collection = db.CreateCollection<Data, int>(d => d.ID);


// save & update

        await collection.SaveAsync(new Data { ID = 1, Name = "a" });
        await collection.SaveAsync(new Data { ID = 1, Name = "x" });


// load with primary key(ID=1)
        
        var data = await collection.LoadAsync(1);

//////////////////////////////////
Auto-Link example
//////////////////////////////////

        var db = new SayDB("c:/tmp/db");
        var mainCollection = db.CreateCollection<Data, int>(d => d.ID);
        var subCollection = db.CreateCollection<Sub, int>(d => d.Key);

        await mainCollection.SaveAsync(new Data { ID = 3, Name = "a", Sub = new Sub { Key = 1, Title = "s1" } });

        var sub = await subCollection.LoadAsync(1);
        
        Assert.AreEqual("s1", sub.Title);

//////////////////////////////////
Query example using primary keys in memory
//////////////////////////////////

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


//////////////////////////////////
Mutual reference auto-resolving example
//////////////////////////////////

        var db = new SayDB("c:/tmp/db");
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


