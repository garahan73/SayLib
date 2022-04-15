using Say32.DB.Core;
using Say32.DB.Core.Database;
using Say32.DB.Core.Serialization;
using Say32.DB.IO;
using Say32.DB.Object.Serialization;
using Say32.DB.Store;
using Say32.DB.Store.IO;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Say32.DB.Object
{
    public class DbClassObjectType : DbObjectType
    {

        public override Type ObjectType => _objType;

        internal DbClassObjectType(Type objectType, DataStoreID store, DbCore db)
        {
            _objType = objectType;
            _store = store;

            Init(objectType, db);
        }

        private readonly DataStoreID _store;
        private readonly Type _objType;

        public override DataStoreID StoreID => _store;

        public override Dictionary<string, PropertyAccessor> PropertyAccessors { get; } = new Dictionary<string, PropertyAccessor>();

        private void Init(Type type, DbCore db)
        {
            var PlatformAdapter = DbWorkSpace.PlatformAdapter;
            var IgnoreAttribute = DbWorkSpace.IgnoreAttribute;

            //Debug.WriteLine($">---- Caching properties for type {type.Name}....");
            Debug.Indent();

            var isList = PlatformAdapter.IsAssignableFrom(typeof(IList), type);
            var isDictionary = PlatformAdapter.IsAssignableFrom(typeof(IDictionary), type);
            var isArray = PlatformAdapter.IsAssignableFrom(typeof(Array), type);

            var noDerived = isList || isDictionary || isArray;

            // first fields
            var fields = from f in PlatformAdapter.GetFields(type)
                         where
                         !f.IsStatic &&
                         !f.IsLiteral &&
                         !f.IsIgnored(IgnoreAttribute) && !f.FieldType.IsIgnored(IgnoreAttribute, PlatformAdapter)
                         select new PropertyOrField(f, PlatformAdapter);

            var properties = from p in PlatformAdapter.GetProperties(type)
                             where
                             (noDerived && p.DeclaringType.Equals(type) || !noDerived) &&
                             p.CanRead && p.CanWrite &&
                             PlatformAdapter.GetGetMethod(p) != null && PlatformAdapter.GetSetMethod(p) != null
                                   && !p.IsIgnored(IgnoreAttribute) && !p.PropertyType.IsIgnored(IgnoreAttribute, PlatformAdapter)
                             select new PropertyOrField(p, PlatformAdapter);

#if DEBUG
            Console.WriteLine($"Get fields and properties of type {type.Name}");
            Console.WriteLine($"{type.Name} Field count = {fields.ToList().Count}");
            fields.ToList().ForEach(f => Console.WriteLine($"- {f.Name}"));
            Console.WriteLine($"{type.Name} Properties count = {properties.ToList().Count}");
            properties.ToList().ForEach(p => Console.WriteLine($"- {p.Name}"));
            Console.WriteLine();
#endif

            foreach (var p in properties.Concat(fields))
            {
                var propType = p.PropertyFieldType;

                //Debug.WriteLine($"[caching prop.] {type.Name}.{p.Name}, type={propType.Name}");

                //TODO: customize way of getting property store from attribute
                var propStore = DataStoreID.FromType(p.PropertyFieldType);
                propStore = db.StoreMap.Has(propStore) ? propStore : null;

                var accessor = new PropertyAccessor(p.Name, propType, (parent, property) => p.ValueSetter(parent, property), p.GetValue, propStore);

                PropertyAccessors.Add(p.Name, accessor);
            }

            Debug.Unindent();
            //Debug.WriteLine("<----- Caching properties done.");

        }

        public void InitForLoad(DataStoreID store) { }

        public override object CreateEmptyObject(BinaryReader br = null)
        {
            return IoHelper.CreateObject(ObjectType);
        }


    }
}