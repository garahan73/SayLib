using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Say32
{
    public static class ReflectionUtil
    {
        public static bool IsEnumerableType(Type type)
        {
            return (type.GetInterface(nameof(IEnumerable)) != null);
        }

        public static bool IsCollectionType(Type type)
        {
            return (type.GetInterface(nameof(ICollection)) != null);
        }

        public static bool IsSingleGenericInterfaceType(Type type, Type genericType) => GetSingleGenericInterfaceArgument(type, genericType) != null;


        public static Type? GetSingleGenericInterfaceArgument(Type type, Type genericType)
        {
            foreach (Type interfaceType in type.GetInterfaces())
            {
                if (interfaceType.IsGenericType &&
                    interfaceType.GetGenericTypeDefinition() == genericType)
                {
                    return interfaceType.GetGenericArguments()[0];
                }
            }

            return null;
        }

        public static bool IsIList(Type type) => IsSingleGenericInterfaceType(type, typeof(IList<>));

        public static IEnumerable<Type> GetTypesOfNamespace(Assembly assembly, string @namespace)
            => from t in assembly.GetTypes()
               where t.IsClass && t.Namespace == @namespace 
               select t;

        public static IEnumerable<Type> GetColleagueTypesOfNameSpace(Type t) => GetTypesOfNamespace(t.Assembly, t.Namespace);

        public static IEnumerable<MethodInfo> GetExtensionMethods( this Type extendedType )
        {
            return from assembly in AppDomain.CurrentDomain.GetAssemblies()
                   from method in GetExtensionMethods(extendedType, assembly)
                   select method;
        }

        public static IEnumerable<MethodInfo> GetExtensionMethods( this Type extendedType, Assembly assembly  )
        {
            return from type in assembly.GetTypes()
                        where type.IsSealed && !type.IsGenericType && !type.IsNested
                        from method in type.GetMethods(BindingFlags.Static
                            | BindingFlags.Public | BindingFlags.NonPublic)
                        where method.IsDefined(typeof(ExtensionAttribute), false)
                        where method.GetParameters()[0].ParameterType == extendedType
                        select method;
        }

        public static MethodInfo? GetExtensionMethod( this Type extendedType, string methodName, params string[] typeNameFilters )
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies()
                    .Where(a=>!a.FullName.StartsWith("System.") && !a.FullName.StartsWith("Microsoft.")))
            {
                var method = GetExtensionMethod(extendedType, assembly, methodName, typeNameFilters);
                if (method != null) return method;
            }
            return null;
        }

        public static MethodInfo? GetExtensionMethod( this Type extendedType, Assembly assembly, string methodName, params string[] typeNameFilters )
        {
            Debug.WriteLine($"###### {assembly}");

            foreach (var type in assembly.GetTypes())
            {
                if (typeNameFilters.Length != 0 && !typeNameFilters.Where(f => type.FullName.Contains(f)).Any())
                    continue;

                if (type.IsAbstract && type.IsSealed && !type.IsGenericType && !type.IsNested)
                {
                    Debug.WriteLine(type);

                    foreach (var method in type.GetMethods())
                    {
                        if (!method.IsStatic) continue;
                        // include non public method?

                        if (method.Name != methodName) continue;

                        if (method.IsDefined(typeof(ExtensionAttribute), false) &&
                            method.GetParameters()[0].ParameterType == extendedType)
                            return method;

                    }
                }
            }
            return null;
        }
    }
}
