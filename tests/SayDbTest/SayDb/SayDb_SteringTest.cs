using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Say32.DB;
using Say32.DB.Core.Database;
using Say32.DB.Server.FileSystem;

namespace Say32.DB
{
    [TestClass]
    public class SayDb_BasicTest
    {
        public class Person
        {
            public int Id;
            public string Name { get; internal set; }
            public int Age;
        }

        class Person2 :Person
        {
            public List<Person2> Children { get; set; } = new List<Person2>();
        }

        public SayDb_BasicTest()
        {
            DbWorkSpace.DataFolder = "c:/tmp/saydb";
        }

        [TestCleanup]
        public void CleanupDbDirectory()
        {
            var fsHelper = new FileSystemHelper();
            CodeUtil.SafeRun(() => fsHelper.Purge(DbWorkSpace.DataFolder));
        }

        [TestMethod]
        public async Task BasicTest()
        {
            using (var engine = DbWorkSpace.Open())
            {
                var db = DbWorkSpace.CreateDatabase("person", DbStorageType.File);
                var table = db.CreateDataStore<Person, int>(testModel => testModel.Id)
                               .WithIndex("name_index", p => p.Name)
                               .WithIndex("age_index", p => p.Age)
                               .WithIndex("complex_index", t => (t.Name, t.Age));
                               
                //db.PublishTables();

                // data
                var sehi = new Person { Id = 10, Name = "sehi", Age = 47 };

                // save 
                var key = db.Save(sehi);
                Assert.AreEqual(10, key);

                // load
                var loaded = db.Load<Person>(key);
                Assert.AreEqual(key, loaded.Id);
                Assert.AreEqual(sehi.Name, loaded.Name);
                Assert.AreEqual(sehi.Age, loaded.Age);

                var gahrahm = new Person { Id = 11, Name = "Gah-Rahm", Age = 8 };
                var gahohn = new Person { Id = 12, Name = "Gah-Ohn", Age = 8 };

                db.Save(gahrahm, gahohn);


                // query by linq
                var twins = await from k in db.IndexQuery<Person, int, int>("age_index")
                                  where k.Index < 10
                                  select k.Value;

                Assert.AreEqual(2, twins.Count());
                Assert.IsTrue(twins.ToArray()[0].Name.StartsWith("Gah-"));

                var jh = new Person { Id = 13, Name = "Jee-Hyun", Age = 46 };
                db.Save(jh);

                // query by complex index
                var list = await from k in db.IndexQuery<Person, (string name, int age), int>("complex_index")
                                 where k.Index.name.StartsWith("J") && k.Index.age > 10
                                 select k.Value;

                Assert.AreEqual(1, list.Count());
                Assert.AreEqual(jh.Name, list.ToArray()[0].Name);
            }

            
        }

        [TestMethod]
        public async Task ComplexObjectTest()
        {
            using (var engine = DbWorkSpace.Open())
            {
                var db = DbWorkSpace.CreateDatabase("person2", DbStorageType.File);
                var table = db.CreateDataStore<Person2, int>(testModel => testModel.Id)
                               .WithIndex("name_index", p => p.Name)
                               .WithIndex("age_index", p => p.Age)
                               .WithIndex("complex_index", t => (t.Name, t.Age));
                               
                //db.PublishTables();

                // data
                var sehi = new Person2 { Id = 10, Name = "sehi", Age = 47 };
                var jh = new Person2 { Id = 11, Name = "Jee-Hyun", Age = 46 };
                var gahrahm = new Person2 { Id = 21, Name = "Gah-Rahm", Age = 8 };
                var gahohn = new Person2 { Id = 22, Name = "Gah-Ohn", Age = 8 };

                sehi.Children.Add(gahohn, gahrahm);
                jh.Children.Add(gahohn, gahrahm);

                // save 
                var key = db.Save(sehi);
                Assert.AreEqual(10, key);

                // load children
                Assert.AreEqual(gahohn.Name, db.Load<Person2>(gahohn.Id).Name);
                Assert.AreEqual(gahrahm.Name, db.Load<Person2>(gahrahm.Id).Name);

                // load
                var sh = db.Load<Person2>(key);
                Assert.AreEqual(key, sh.Id);
                Assert.AreEqual(sehi.Name, sh.Name);
                Assert.AreEqual(sehi.Age, sh.Age);

                // check loaded children
                Assert.AreEqual(2, sh.Children.Count);
                Assert.AreEqual(2, sh.Children.Where(c => c.Age == 8).Count());
                Assert.AreEqual(gahrahm.Id, sh.Children.Where(c => c.Name == "Gah-Rahm").First().Id);
                Assert.AreEqual(0, sh.Children.Where(c => c.Name == "Gah-Rahm").First().Children.Count);

                // query by children
                var twins = await from k in db.IndexQuery<Person2, int, int>("age_index")
                                  where k.Index < 10
                                  select k.Value;

                Assert.AreEqual(2, twins.Count());
                Assert.IsTrue(twins.ToArray()[0].Name.StartsWith("Gah-"));
                
                db.Save(jh);


                Debug.WriteLine("#################### complex index query #######################");

                // query by complex index
                var list = await from k in db.IndexQuery<Person2, (string name, int age), int>("complex_index")
                                 where k.Index.name.StartsWith("J") && k.Index.age > 10
                                 select k.Value;

                Assert.AreEqual(1, list.Count());
                Assert.AreEqual(jh.Name, list.ToArray()[0].Name);
            }
        }

        [TestMethod]
        public void LoadTest()
        {
            using (var engine = DbWorkSpace.Open())
            {
                var db = DbWorkSpace.CreateDatabase("person2", DbStorageType.File);
                var store = db.CreateDataStore<Person2, int>(testModel => testModel.Id)
                               .WithIndex("name_index", p => p.Name)
                               .WithIndex("age_index", p => p.Age)
                               .WithIndex("complex_index", t => (t.Name, t.Age))
                               ;

                //db.PublishTables();

                const int test_count = 100;

                var tasks = new Task[test_count];

                for (int i = 0; i < test_count; i++)
                {
                    var sehi = new Person2 { Id = i, Name = $"person_{i}", Age = 10 };
                    tasks[i] = store.SaveAsync(sehi);                    
                }

                Assert.IsTrue(Task.WaitAll(tasks, 10 * 1000));
            }
        }

        class CrossReference
        {
            public int Id;
            public CrossReference Ref;            
        }

        [TestMethod]
        public void CrossReferenceTest()
        {
            using (var engine = DbWorkSpace.Open())
            {
                var db = DbWorkSpace.CreateDatabase("person2", DbStorageType.File);
                var store = db.CreateDataStore<CrossReference, int>(o => o.Id);

                // data
                var a = new CrossReference { Id = 10 };
                var b = new CrossReference { Id = 11 };

                a.Ref = b;

                // save a + b;
                var key = db.Save(a);
                Assert.AreEqual(10, key);

                // load b
                Assert.AreEqual(b.Id, store.Load(b.Id).Id);

                // load a check a.Ref is b
                Assert.AreEqual(b.Id, store.Load(a.Id).Ref.Id);

                // set cross reference
                b.Ref = a;
                db.Save(b);

                // load b check b.Ref is a
                Assert.AreEqual(a.Id, store.Load(b.Id).Ref.Id);

                // load a check a.Ref is b
                Assert.AreEqual(b.Id, store.Load(a.Id).Ref.Id);
            }
        }
        [TestMethod]
        public async Task NewTupleIndexTest()
        {
            using (var engine = DbWorkSpace.Open())
            {                
                var db = DbWorkSpace.CreateDatabase("person2", DbStorageType.File);
                db.CreateDataStore<Person2, int>(p => p.Id)
                    //.WithIndex("old_tuple_index", p => Tuple.Create(p.Name, p.Age))
                    .WithIndex("new_tuple_index", p=>(p.Name, p.Age));

                //db.PublishTables();

                // data
                var sehi = new Person2 { Id = 10, Name = "sehi", Age = 47 };
                var jeehyun = new Person2 { Id = 13, Name = "Jee-Hyun", Age = 46 };
                var gahrahm = new Person2 { Id = 11, Name = "Gah-Rahm", Age = 8 };
                var gahohn = new Person2 { Id = 12, Name = "Gah-Ohn", Age = 8 };

                sehi.Children.Add(gahohn, gahrahm);
                jeehyun.Children.Add(gahohn, gahrahm);

                // save 
                db.Save(sehi, jeehyun);

                // query by children
                var twins = await from k in db.IndexQuery<Person2, (string name, int age), int>("new_tuple_index")
                                  where k.Index.age < 10 && k.Index.name.StartsWith("Gah-")
                                  select k.Value;

                Assert.AreEqual(2, twins.Count());
                twins.ToList().ForEach(p=> Assert.IsTrue(p.Name.StartsWith("Gah-")));

            }
        }

        [TestMethod]
        public void TupleSerialization()
        {
            var tuple = (name:"any name", age: 20);

            var formatter = new BinaryFormatter();
            using (var stream = new MemoryStream())
            {
                formatter.Serialize(stream, tuple);

                stream.Seek(0, SeekOrigin.Begin);

                var loaded = ((string name, int age))formatter.Deserialize(stream);
                Assert.AreEqual(tuple, loaded);
            }            
        }

        private class PrivateSetterClass
        {
            private PrivateSetterClass()
            {
            }

            public int ID { get; private set; }
            public PrivateSetterClass(int id)
            {
                ID = id;
            }

        }

        private class PrivateCtorClass
        {
            private PrivateCtorClass()
            {                    
            }
        }

        [TestMethod]
        public void PrivateConstructorTest()
        {
            //var obj = Activator.CreateInstance(typeof(PrivateCtorClass));
            var ctor = typeof(PrivateCtorClass).GetConstructor(BindingFlags.Instance|BindingFlags.NonPublic, null, new Type[] { }, new ParameterModifier[] { });
            Assert.IsNotNull(ctor);            
            var obj = ctor.Invoke(new object[] { }) as PrivateCtorClass;
            Assert.IsNotNull(obj);
        }

        [TestMethod]
        public void PrivateSetterTest()
        {
            var t = typeof(PrivateSetterClass);

            var fs  =t.GetFields(BindingFlags.NonPublic);
            //Assert.AreEqual(1, fs.Length);

            var ps = t.GetProperties();
            Assert.AreEqual(1, ps.Length);
            var p = ps[0];
            Assert.IsTrue(p.CanRead);
            Assert.IsTrue(p.CanWrite);
            Assert.IsNotNull(p.GetSetMethod(true));

            using (var engine = DbWorkSpace.Open())
            {
                var db = DbWorkSpace.CreateDatabase("pset");
                db.CreateDataStore<PrivateSetterClass, int>(x => x.ID);

                //db.PublishTables();

                // data
                var org = new PrivateSetterClass(10);

                // save 
                db.Save(org);

                // query by children
                var l = db.Load<PrivateSetterClass>(org.ID);
                Assert.AreEqual(org.ID, l.ID);
            }
        }

        /*
        [TestMethod]
        public void ValueTupleTypeCheck()
        {
            var tuple = (name: "any name", age: 20);

            Assert.IsTrue(tuple is ITuple);
            Assert.IsTrue(Tuple.Create("name", 10) is ITuple);
            Assert.IsTrue(typeof(ValueTuple<string, int>).Equals(typeof((string, int))));
            Assert.IsTrue(typeof(ValueTuple).IsAssignableFrom(typeof(ValueTuple<string, int>)));
            Assert.IsTrue(tuple is ValueTuple);
            
        }
        */

        private enum TestEnum
        {
            AAAA, BBBB
        }

        class EnumTestClass
        {
            public TestEnum field;
        }

        [TestMethod]
        public void EnumTest()
        {
            using (var engine = DbWorkSpace.Open())
            {
                var db = DbWorkSpace.CreateDatabase("enum");
                db.CreateDataStore<EnumTestClass, TestEnum>(x => x.field);

                //db.PublishTables();

                // data
                var org = new EnumTestClass { field = TestEnum.BBBB };

                // save 
                db.Save(org);

                // query by children
                Assert.AreEqual(org.field, db.Load<EnumTestClass>(org.field).field);
            }
        }

        class ArrayClass
        {
            public int ID;
            public int[] Array;
        }

        [TestMethod]
        public void ArrayPropTest()
        {
            var org = new ArrayClass
            {
                ID = 10,
                Array = new int[] { 1, 2, 3 }
            };

            using (var engine = DbWorkSpace.Open())
            {
                var db = DbWorkSpace.CreateDatabase("temp1");
                var store = db.CreateDataStore<ArrayClass, int>(x => x.ID);

                store.Save(org);

                var obj = store.Load(org.ID);
                Assert.AreEqual(3, obj.Array.Length);
                Assert.AreEqual(1, obj.Array[0]);
                Assert.AreEqual(2, obj.Array[1]);
                Assert.AreEqual(3, obj.Array[2]);
            }
        }

        class ListClass
        {
            public int ID;
            public List<int> List;
        }

        [TestMethod]
        public void ListPropTest()
        {
            var org = new ListClass
            {
                ID = 10,
                List = new List<int> { 1, 2, 3 }
            };

            using (var engine = DbWorkSpace.Open())
            {
                var db = DbWorkSpace.CreateDatabase("temp1");
                var store = db.CreateDataStore<ListClass, int>(x => x.ID);

                store.Save(org);

                var obj = store.Load(org.ID);
                Assert.AreEqual(3, obj.List.Count);
                Assert.AreEqual(1, obj.List[0]);
                Assert.AreEqual(2, obj.List[1]);
                Assert.AreEqual(3, obj.List[2]);
            }
        }

        class DicClass
        {
            public int ID;
            public Dictionary<string, object> Dic;
        }

        [TestMethod]
        public void DictionaryPropTest()
        {
            var org = new DicClass
            {
                ID = 10,
                Dic = new Dictionary<string, object>() {
                        { "se-hi", 47 },
                        { "jee-hyun", 46 }
                    },
            };

            using (var engine = DbWorkSpace.Open())
            {
                var db = DbWorkSpace.CreateDatabase("temp1");
                var store = db.CreateDataStore<DicClass, int>(x => x.ID);

                store.Save(org);

                var obj = store.Load(org.ID);
                Assert.AreEqual(2, obj.Dic.Count);
                Assert.AreEqual(47, obj.Dic["se-hi"]);
                Assert.AreEqual(46, obj.Dic["jee-hyun"]);
            }
        }
    }


}

