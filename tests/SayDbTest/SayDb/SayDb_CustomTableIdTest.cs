using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Say32.DB;
using Say32.DB.Server.FileSystem;
using Say32.DB.Store;

#pragma warning disable CS0659, CS0162

namespace Say32.DB
{
    [TestClass]
    public class SayDb_CustomStoreTest
    {
        private const string DbName = "customTID";
        private const string DB_FOLDER = "c:/tmp/saydb";

        public class Person
        {
            public int Id;
            public string Name { get; internal set; }
            public int Age;

            public override bool Equals(object obj)
            {
                return obj != null
                       && obj is Person p
                       && Id == p.Id
                       && Name == p.Name
                       && Age == p.Age;
            }
        }
        

        class Person2 :Person
        {
            public List<Person2> Children { get; set; } = new List<Person2>();

            public override bool Equals(object obj)
            {
                return base.Equals(obj)
                       && obj is Person2 person
                       && ChildrenEquals(person.Children);
            }

            private bool ChildrenEquals(List<Person2> children)
            {
                if (Children == null)
                    return children == null;

                if (Children.Count != children.Count)
                    return false;

                foreach (var child in Children)
                {
                    if (!children.Where(c => c.Id == child.Id).Any())
                        return false;
                }
                return true;
            }
        }

        class Person3 : Person2
        {
            public Person3 Spouse { get; internal set; }

            public override bool Equals(object obj)
            {
                return base.Equals(obj)
                       && obj is Person3 p2
                       && (Spouse?.Id == p2.Spouse?.Id);
            }
        }

        [TestCleanup]
        public void CleanupDbDirectory()
        {
            var fsHelper = new FileSystemHelper();
            fsHelper.Purge(DB_FOLDER);
        }

        [TestMethod]
        public async Task Person1Test()
        {
            CleanupDbDirectory();

            using (DbWorkSpace.Open())
            {
                var db = DbWorkSpace.CreateDatabase(DbName, DbStorageType.File, DB_FOLDER);
                var store = db.CreateDataStore<Person, int>(DataStoreID.FromName("person1"), p => p.Id)                    
                               .WithIndex("name_index", p => p.Name)
                               .WithIndex("age_index", p => p.Age)
                               .WithIndex("complex_index", p => (p.Name, p.Age));
                
                // data
                var sehi = new Person { Id = 10, Name = "sehi", Age = 47 };
                var jeehyun = new Person { Id = 11, Name = "Jee-Hyun", Age = 46 };
                var gahrahm = new Person { Id = 12, Name = "Gah-Rahm", Age = 8 };
                var gahohn = new Person { Id = 13, Name = "Gah-Ohn", Age = 8 };

                // save 
                var key = store.Save(sehi);
                Assert.AreEqual(10, key);

                // load
                var sh = store.Load(key);
                Assert.AreEqual(key, sh.Id);
                Assert.AreEqual(sehi.Name, sh.Name);
                Assert.AreEqual(sehi.Age, sh.Age);

                store.Save(gahrahm, gahohn);

                // query by key
                var all = await from p in db.KeyQuery<Person, int>(store.ID)
                           select p.Value;

                Assert.AreEqual(3, all.Count());

                // query by index
                var twins = await from k in db.IndexQuery<Person, int, int>(store.ID, "age_index")
                             where k.Index < 10
                             select k.Value;

                Assert.AreEqual(2, twins.Count());
                Assert.IsTrue(twins.ToList()[0].Name.StartsWith("Gah-"));


                store.Save(jeehyun);

                // query by complex index
                var list = await from k in db.IndexQuery<Person, (string name , int age), int>(store.ID, "complex_index")
                              where k.Index.name.StartsWith("J") && k.Index.age > 10
                              select k.Value;

                Assert.AreEqual(1, list.Count());
                Assert.AreEqual(jeehyun.Name, list.ToList()[0].Name);

                // query using store
                all = await from k in store.KeyQuery()
                            select k.Value;

                Assert.AreEqual(4, all.Count());

                twins = await from k in store.IndexQuery<int>("age_index")
                              where k.Index < 10
                              select k.Value;

                Assert.AreEqual(2, twins.Count());
                Assert.IsTrue(twins.ToList()[0].Name.StartsWith("Gah-"));

                list = await from k in store.IndexQuery<(string name, int age)>("complex_index")
                             where k.Index.name.StartsWith("J") && k.Index.age > 10
                             select k.Value;

                Assert.AreEqual(1, list.Count());
                Assert.AreEqual(jeehyun.Name, list.ToList()[0].Name);
            }            
        }

        [TestMethod]
        public void DualTypeStoreTest()
        {
            using (DbWorkSpace.Open())
            {
                var db = DbWorkSpace.CreateDatabase(DbName, DbStorageType.File, DB_FOLDER);
                var store0 = db.CreateDataStore<Person, int>(DataStoreID.FromName("person1"), p => p.Id)
                               .WithIndex("name_index", p => p.Name)
                               .WithIndex("age_index", p => p.Age)
                               .WithIndex("complex_index", p => (p.Name, p.Age));

                var store1 = db.CreateDataStore<Person, int>(p => p.Id)
                               .WithIndex("name_index", p => p.Name)
                               .WithIndex("age_index", p => p.Age)
                               .WithIndex("complex_index", p => (p.Name, p.Age));

                // data
                var sehi = new Person { Id = 10, Name = "sehi", Age = 47 };
                var gahrahm = new Person { Id = 11, Name = "Gah-Rahm", Age = 8 };
                var gahohn = new Person { Id = 12, Name = "Gah-Ohn", Age = 8 };

                // save differently per store
                store0.Save(sehi);
                store1.Save(sehi, gahrahm, gahohn);

                //load
                var all0 = (from p in db.KeyQuery<Person, int>(store0.ID) select p.Value).WaitAllTasks().ToList();
                var all1 = (from p in db.KeyQuery<Person, int>(store1.ID) select p.Value).WaitAllTasks().ToList();

                // assert
                Assert.AreEqual(1, all0.Count);
                Assert.AreEqual(3, all1.Count);
            }

        }

        [Timeout(500)]
        [TestMethod]
        public void Person3Compare()
        {
            var a = new Person3 { Name = "a" };
            var b = new Person3 { Name = "b" };

            a.Spouse = b;
            b.Spouse = a;

            Assert.AreEqual(a, new Person3 { Name = "a", Spouse = b });

            var c = new Person3 { Name = "c" };
            a.Children.Add(c);
            b.Children.Add(c);

            Assert.AreEqual(a, new Person3 { Name = "a", Spouse = b, Children = new List<Person2> { c } });
        }

        //[Timeout(10000)]
        [TestMethod]
        public async Task Person3Test()
        {
            using (DbWorkSpace.Open())
            {
                var db = DbWorkSpace.CreateDatabase(DbName, DbStorageType.File, DB_FOLDER);
                var store = db.CreateDataStore<Person3, int>("person2", testModel => testModel.Id)                                
                               .WithIndex("name_index", p => p.Name)
                               .WithIndex("age_index", p => p.Age)
                               .WithIndex("complex_index", t => (t.Name, t.Age));
                //store.MapType<Person3>(store.ID);

                // data
                var sehi = new Person3 { Id = 10, Name = "sehi", Age = 47 };
                var jeehyun = new Person3 { Id = 11, Name = "Jee-Hyun", Age = 46 };
                var gahrahm = new Person3 { Id = 21, Name = "Gah-Rahm", Age = 8 };
                var gahohn = new Person3 { Id = 22, Name = "Gah-Ohn", Age = 8 };

                // spouse
                sehi.Spouse = jeehyun;
                jeehyun.Spouse = sehi;

                store.Save(sehi, jeehyun);
                var sh = store.Load(sehi.Id);
                Assert.AreEqual(jeehyun.Id, sh.Spouse.Id);

                Assert.AreEqual(2, store.Count);

                var jh = store.Load(jeehyun.Id);
                Assert.AreEqual(sehi.Id, jh.Spouse.Id);

                // children
                sehi.Children.Add(gahohn, gahrahm);
                jeehyun.Children.Add(gahohn, gahrahm);

                store.Save(sehi, jeehyun);
                Assert.AreEqual(4, store.Count);

                // load children
                Assert.AreEqual(gahohn, store.Load(gahohn.Id));
                Assert.AreEqual(gahrahm, store.Load(gahrahm.Id));

                sh = store.Load(sehi.Id);
                //Assert.AreEqual(sehi, sh);

                // check loaded children
                Assert.AreEqual(2, sh.Children.Count);
                Assert.AreEqual(2, sh.Children.Where(c => c.Age == 8).Count());
                Assert.AreEqual(gahrahm.Id, sh.Children.Where(c => c.Name == "Gah-Rahm").First().Id);
                Assert.AreEqual(0, sh.Children.Where(c => c.Name == "Gah-Rahm").First().Children.Count);

                jh = store.Load(jeehyun.Id);
                Assert.AreEqual(2, jh.Children.Count);

                // query by children
                var twins = (from k in db.IndexQuery<Person3, int, int>(store.ID, "age_index")
                             where k.Index < 10
                             select k.Value).WaitAllTasks().ToList();

                Assert.AreEqual(2, twins.Count);
                Assert.IsTrue(twins[0].Name.StartsWith("Gah-"));

                
                store.Save(jeehyun);

                // query by complex index
                var list = await from k in store.IndexQuery<(string name, int age)>("complex_index")
                                 where k.Index.name.StartsWith("J") && k.Index.age > 10
                                 select k.Value;
                Assert.AreEqual(1, list.Count());
                Assert.AreEqual(jeehyun.Name, list.ToList()[0].Name);
            }
        }

        //[Timeout(10000)]
        [TestMethod]
        public void Person3Test_WithoutQuery()
        {
            using (var engine = DbWorkSpace.Open())
            {
                var db = DbWorkSpace.CreateDatabase(DbName, DbStorageType.File, DB_FOLDER);
                var store = db.CreateDataStore<Person3, int>("person2", testModel => testModel.Id)
                               .WithIndex("name_index", p => p.Name)
                               .WithIndex("age_index", p => p.Age)
                               .WithIndex("complex_index", t => (t.Name, t.Age));
                //store.MapType<Person3>(store.ID);

                // data
                var sehi = new Person3 { Id = 10, Name = "sehi", Age = 47 };
                var jeehyun = new Person3 { Id = 11, Name = "Jee-Hyun", Age = 46 };
                var gahrahm = new Person3 { Id = 21, Name = "Gah-Rahm", Age = 8 };
                var gahohn = new Person3 { Id = 22, Name = "Gah-Ohn", Age = 8 };

                // spouse
                sehi.Spouse = jeehyun;
                jeehyun.Spouse = sehi;

                //store.Save(sehi);
                store.Save(sehi, jeehyun);

                var sh = store.Load(sehi.Id);
                Assert.AreEqual(jeehyun.Id, sh.Spouse.Id);
                
                Assert.AreEqual(2, store.Count);

                var jh = store.Load(jeehyun.Id);
                Assert.AreEqual(sehi.Id, jh.Spouse.Id);

                // children
                sehi.Children.Add(gahohn, gahrahm);
                jeehyun.Children.Add(gahohn, gahrahm);
                
                store.Save(sehi, jeehyun);
                
                Assert.AreEqual(4, store.Count);
                return;

                // load children
                Assert.AreEqual(gahohn, store.Load(gahohn.Id));
                Assert.AreEqual(gahrahm, store.Load(gahrahm.Id));

                sh = store.Load(sehi.Id);
                //Assert.AreEqual(sehi, sh);

                // check loaded children
                Assert.AreEqual(2, sh.Children.Count);
                Assert.AreEqual(2, sh.Children.Where(c => c.Age == 8).Count());
                Assert.AreEqual(gahrahm.Id, sh.Children.Where(c => c.Name == "Gah-Rahm").First().Id);
                Assert.AreEqual(0, sh.Children.Where(c => c.Name == "Gah-Rahm").First().Children.Count);

                jh = store.Load(jeehyun.Id);
                Assert.AreEqual(2, jh.Children.Count);

                store.Save(jeehyun);
            }
        }


        [TestMethod]
        public async Task NewTupleIndexTest()
        {
            using (var engine = DbWorkSpace.Open())
            {                
                var db = DbWorkSpace.CreateDatabase(DbName, DbStorageType.File, DB_FOLDER);
                var store = db.CreateDataStore<Person3, int>("person2", p => p.Id)
                    //.WithIndex("old_tuple_index", p => Tuple.Create(p.Name, p.Age))
                    .WithIndex("new_tuple_index", p=>(p.Name, p.Age));
                //store.MapType<Person2>(store.ID); <- auto for self type mapping



                // data
                var sehi = new Person3 { Id = 10, Name = "sehi", Age = 47 };
                var jeehyun = new Person3 { Id = 13, Name = "Jee-Hyun", Age = 46 };
                var gahrahm = new Person3 { Id = 11, Name = "Gah-Rahm", Age = 8 };
                var gahohn = new Person3 { Id = 12, Name = "Gah-Ohn", Age = 8 };

                sehi.Children.Add(gahohn, gahrahm);
                jeehyun.Children.Add(gahohn, gahrahm);

                // save 
                await store.SaveAsync(sehi, jeehyun);

                Assert.AreEqual(4, store.Count);

                // query by children
                var twins = await from k in store.IndexQuery<(string name, int age)>("new_tuple_index")
                             where k.Index.age < 10 && k.Index.name.StartsWith("Gah-")
                             select k.Value;

                Assert.AreEqual(2, twins.Count());
                foreach (var person in twins)
                {
                    Assert.IsTrue(person.Name.StartsWith("Gah-"));
                }
                

            }
        }

        private class DicClass
        {
            public int ID;
            public IDictionary<string, object> Dic { get; set; }
            public IList<int> List;
            public int[] Array;

            public IDictionary<string, object> NullDic { get; set; }
            public IList<int> NullList = null;
            public int[] NullArray = null;

        }

        [TestMethod]
        public void DicListArrayTest()
        {
            using (var engine = DbWorkSpace.Open())
            {
                var db = DbWorkSpace.CreateDatabase("dic");
                var store = db.CreateDataStore<DicClass, int>("dic", x => x.ID);

                //db.PublishTables();

                // data
                var org = new DicClass
                {
                    ID = 10,
                    Dic = new Dictionary<string, object>() {
                        { "se-hi", 47 },
                        { "jee-hyun", 46 }
                    },
                    List = new List<int> { 1, 2, 3 },
                    Array = new int[] { 10, 9 }
                };
                
                // save 
                store.Save(org);

                // query by children
                var loaded = store.Load(org.ID);
                Assert.AreEqual(org.ID, loaded.ID);

                // dictionary
                Assert.AreEqual(2, loaded.Dic.Count);
                Assert.AreEqual(47, loaded.Dic["se-hi"]);
                Assert.AreEqual(46, loaded.Dic["jee-hyun"]);

                // list
                Assert.AreEqual(3, loaded.List.Count);
                Assert.AreEqual(1, loaded.List[0]);
                Assert.AreEqual(2, loaded.List[1]);
                Assert.AreEqual(3, loaded.List[2]);

                // array
                Assert.AreEqual(2, loaded.Array.Length);
                Assert.AreEqual(10, loaded.Array[0]);
                Assert.AreEqual(9, loaded.Array[1]);

                // check null
                Assert.IsNull(loaded.NullDic);
                Assert.IsNull(loaded.NullArray);
                Assert.IsNull(loaded.NullList);


            }

        }
    }


}

