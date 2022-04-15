using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Say32
{
    public static class TypeUtil
    {
        public static bool IsPrimitiveExt(this Type type, params Type[] additionalPrimitiveTypes) => type.IsPrimitive || type.IsEnum ||
            type == typeof(string) || type == typeof(decimal) ||
            additionalPrimitiveTypes.Where(t => type == t).Any();

        public static bool IsCollection(this Type type) => TypeUtil.HasGenericInterface(type, typeof(ICollection<>));

        public static IEnumerable<Type> GetBaseClassesAndInterfaces( this Type type, bool includeInterfaces = true, Func<Type, bool>? filter = null )
        {
            //Debug.WriteLine(DebugHelper.GetDegbugMessage(typeof(TypeExtensions), nameof(GetBaseClassesAndInterfaces),  $" type =  '{type.FullName}'"));

            var result = 
                // interface
                type.BaseType == null ? Enumerable.Empty<Type>() :
                // last
                type.BaseType == typeof(object) ? (includeInterfaces ? type.GetInterfaces() : Enumerable.Empty<Type>() ) :
                // general
                Enumerable
                    .Repeat(type.BaseType, 1)
                    .If(includeInterfaces, e=>e.Concat(type.GetInterfaces()))                    
                    .Concat(type.BaseType.GetBaseClassesAndInterfaces(includeInterfaces))
                    .Distinct();

            return filter == null ? result :
                    result.Where(t => filter(t));
        }

        public static bool HasGenericInterface(this Type type, Type genericInterfaceType) 
            => type.GetInterfaces().
               Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == genericInterfaceType);
    }
}
