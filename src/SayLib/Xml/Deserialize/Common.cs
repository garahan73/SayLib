using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Say32.Xml.Deserialize
{
    class ObjectCreatorHolder
    {
        protected readonly XmlTypeMap _xmlTypeMap;

        public ObjectCreatorHolder(XmlTypeMap objectCreator)
        {
            _xmlTypeMap = objectCreator;
        }

    }


    public class XmlTypeMapBuilder
    {
        public static Dictionary<string, Type> BuildByReprsentativeTypes(params Type[] representativeTypes ) => BuildByReprsentativeTypes((IEnumerable<Type>)representativeTypes);

        public static Dictionary<string, Type> BuildByReprsentativeTypes(IEnumerable<Type> representativeTypes)
        {
            // remove duplication
            var types = new HashSet<Type>();

            //var referenceTypes = new HashSet<Type>();

            //var namespaces = new HashSet<string>();

            foreach (var type in representativeTypes)
            {

                //if (namespaces.Contains(type.Namespace))
                //continue;

                //namespaces.Add(type.Namespace);
                types.UnionWith(ReflectionUtil.GetColleagueTypesOfNameSpace(type).Where(t =>!t.Name.Contains("<")));
            }

            return types.ToDictionary(t=>t.Name, t=>t);
        }

    }
}
