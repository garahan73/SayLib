using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Say32.Xml.Deserialize
{
    public class XmlTypeMap : Dictionary<string, Type>
    {
        public XmlTypeMap()
        {
        }

        public XmlTypeMap( IDictionary<string, Type> dictionary ) : base(dictionary)
        {
        }

        public Type? GetTypeSafe( string typeName ) => ContainsKey(typeName) ? this[typeName] : null;

        public object CreateEmptyObject(string typeName)
        {
            XmlIoDebug.Deserializer.Log("CREATE OBJECT", typeName);

            var type = GetTypeSafe(typeName) ?? 
                        throw new MissingTypeException($"Can't find matched type from type name '{typeName}'");

            return CreateEmptyObject(type);
        }

        public object CreateEmptyObject( Type type )
        {
            try
            {
                var ctor = type.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new Type[] { }, new ParameterModifier[] { });

                if (ctor == null)
                {
                    return Activator.CreateInstance(type);
                    //throw new SayDbException($"Failed to create object during loading. Type {type.FullName} doesn't have constructor without parameter ");
                }
                else
                    return ctor.Invoke(new object[] { });
            }
            catch (Exception ex)
            {
                var typeName = type.Name;

                XmlIoDebug.Deserializer.Log("CREATE OBJECT FAIL!", typeName);
                XmlIoDebug.Deserializer.SubLog(ex.Message);

                throw new ObjectCreationException($"type = '{typeName}'", ex);
            }
        }

        public Type GetMoreDetailedType( Type type1, string typeName2 )
        {
            if (!ContainsKey(typeName2))
                return type1;

            return GetMoreDetailedType(type1, this[typeName2]);
        }

        public Type GetMoreDetailedType( Type? type1, Type? type2 )
        {
            if (type1 is null && type2 is null)
                throw new Exception($"Both type is null. Can't compare.");

            // non nullable
            if (type1 is null || type2 is null)
            {
#pragma warning disable CS8603 // 가능한 null 참조 반환입니다.
                return type1 is null ? type2 : type1;
#pragma warning restore CS8603 // 가능한 null 참조 반환입니다.
            }

            if (type1 == type2)
                return type1;


            // children
            if (type1.IsAssignableFrom(type2))
                return type2;

            if (type2.IsAssignableFrom(type1))
                return type1;

            // compare generic arguments
            if (type1.IsGenericType && type2.IsGenericType)
            {
                var gargs1 = type1.GetGenericArguments().ToArray();
                var gargs2 = type2.GetGenericArguments().ToArray();
                

                // compare generic arguments counts
                var gcount1 = gargs1.Count();
                var gcount2 = gargs2.Count();

                if (gcount1 != gcount2)
                    return gcount1 > gcount2 ? type1 : type2;
                else
                {
                    for (int i = 0; i < gargs1.Length; i++)
                    {
                        if (gargs2[i] == typeof(object) || gargs1[i] == typeof(object))
                        {
                            return gargs1[i] == typeof(object) ? type2 : type1;
                        }
                    }
                }
            }

            return type1;

        }

        public void Register( Type type )
        {
            this[type.Name] = type;
        }

        //internal string PrintReferenceTypes() => string.Join("\n", _typeMap.Select(t => $"{t.Name}:\t\t {t.FullName}"));
    }



}