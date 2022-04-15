using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Say32.Xml
{
    public class XmlIoDebug
    {
        public static readonly DebugLogger<XmlIoDebugLevel> Serializer = new DebugLogger<XmlIoDebugLevel> { HEADER = "XML SERIALIZE" };
        public static  readonly DebugLogger<XmlIoDebugLevel> Deserializer = new DebugLogger<XmlIoDebugLevel> { HEADER = "XML DESERIALIZE" };
    }

    public enum XmlIoDebugLevel
    {
        INFO=0, DETAIL=1
    }



}
