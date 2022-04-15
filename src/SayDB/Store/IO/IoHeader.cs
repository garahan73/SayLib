using System.Diagnostics;
using System.IO;

namespace Say32.DB.Store.IO
{
    public class IoHeader
    {
        private int _typeCode = -1;
        private string _propName;
        //private int _propertiesCount = -1;

        public bool HasStore => StoreKeyName != null;
        public string StoreKeyName { get; set; }
        public bool IsNull { get; set; }
        public bool IsUsingSerializer { get; set; }
        public bool HasName { get; private set; }
        public bool HasTypeInfo { get; private set; }

        public bool HasProperties => PropertiesCount != -1;


        public bool IsCollection => IsArray || IsList || IsDictionary;
        public bool IsArray { get; set; }
        public bool IsList { get; set; }
        public bool IsDictionary { get; set; }

        //public bool IsArrayOrListItem => Index >= 0;
        //public int Index { get; set; } = -1;

        public int TypeCode
        {
            get => _typeCode;
            set
            {
                _typeCode = value;
                HasTypeInfo = _typeCode != -1;
            }
        }


        public string PropertyName
        {
            get => _propName;
            set
            {
                _propName = value;
                HasName = true;
            }
        }

        public int PropertiesCount { get; set; } = -1;

        public byte AsByte0 => (byte)(
                (IsNull ? 1 : 0) +
                (HasStore ? 2 : 0) +

                (HasName ? 4 : 0) +
                (IsUsingSerializer ? 8 : 0) +
                (HasTypeInfo ? 16 : 0) +

                (IsCollection ? 32 : 0) +
                (HasProperties ? 64 : 0)
            //(IsArrayOrListItem ? 128 : 0)
            );


        public byte AsArrayTypeByte => (byte)(
            (IsArray ? 1 : 0) +
            (IsList ? 2 : 0) +
            (IsDictionary ? 4 : 0)
        );



        public IoHeader()
        {
        }

        private IoHeader(byte b)
        {
            IsNull = (b & 1) != 0;
            _hasStore = (b & 2) != 0;
            HasName = (b & 4) != 0;
            IsUsingSerializer = (b & 8) != 0;
            HasTypeInfo = (b & 16) != 0;
            _isCollection = (b & 32) != 0;
            _hasProperties = (b & 64) != 0;
            _isArrayOrListItem = (b & 128) != 0;
        }

        private readonly bool _isCollection;
        private readonly bool _hasStore;
        private readonly bool _hasProperties;
        private readonly bool _isArrayOrListItem;

        public void Serialize(BinaryWriter bw)
        {
            //Debug.WriteLine($"[{nameof(IoHeader)}] serializing statrted.... ");
            Debug.Indent();

            //Debug.WriteLine($"[code] {this}");


            bw.Write(AsByte0);

            if (HasStore)
            {
                bw.Write(StoreKeyName);
                //Debug.WriteLine($"[store key name] {StoreKeyName}");
            }

            if (HasName)
            {
                bw.Write(PropertyName);
                //Debug.WriteLine($"[property name] {PropertyName}");
            }

            if (HasTypeInfo)
            {
                bw.Write(TypeCode);
                //Debug.WriteLine($"[type code] {TypeCode}");
            }

            if (HasProperties)
            {
                bw.Write(PropertiesCount);
            }

            if (IsCollection)
            {
                bw.Write(AsArrayTypeByte);
                ////Debug.WriteLine($"[{nameof(TypeInfo)}] serialized collection info. {AsByte1}");
            }

            /*
            if (IsArrayOrListItem)
                bw.Write(Index);
                */


            Debug.Unindent();
            //Debug.WriteLine($"[{nameof(IoHeader)}] serialized.");

        }



        public static IoHeader Deserialize(BinaryReader br)
        {
            //Debug.WriteLine($"[{nameof(IoHeader)}] start deserializing.....");
            Debug.Indent();

            try
            {
                var @byte = br.ReadByte();
                var ti = new IoHeader(@byte);

                if (ti._hasStore)
                {
                    ti.StoreKeyName = br.ReadString();
                    //Debug.WriteLine($"[store key name] {ti.StoreKeyName}");
                }

                if (ti.HasName)
                {
                    ti._propName = br.ReadString();
                    //Debug.WriteLine($"[prop. name] {ti.PropertyName}");
                }

                if (ti.HasTypeInfo)
                {
                    ti._typeCode = br.ReadInt32();
                    //Debug.WriteLine($"[type code] {ti.TypeCode}");
                }

                if (ti._hasProperties)
                {
                    ti.PropertiesCount = br.ReadInt32();
                }

                if (ti._isCollection)
                {
                    @byte = br.ReadByte();
                    ti.DeserializeCollectionType(@byte);
                    ////Debug.WriteLine($"[{nameof(TypeInfo)}] deserialized collection info. {@byte}");
                }

                /*
                if (ti._isArrayOrListItem)
                    ti.Index = br.ReadInt32();
                    */

                //Debug.WriteLine($"[code] {ti}");

                return ti;
            }
            finally
            {
                Debug.Unindent();
                //Debug.WriteLine($"[{nameof(IoHeader)}] deserialized.");
            }

        }

        private void DeserializeCollectionType(byte b)
        {
            IsArray = (b & 1) != 0;
            IsList = (b & 2) != 0;
            IsDictionary = (b & 4) != 0;
        }

        public override string ToString()
        {
            return $"[{AsByte0}{(AsArrayTypeByte == 0 ? "" : "," + AsArrayTypeByte)}]" +
(IsNull ? "(NULL)" : "") +

(HasStore ? "(DB object)" : "") +
(HasName ? " (Property)" : "") +
(IsUsingSerializer ? " (Use serializer)" : "") +
(HasTypeInfo ? " (Type specified)" : "") +
(HasProperties ? $"(Class.{PropertiesCount})" : "") +

(IsCollection ? $" (Collection)" : "") +
(IsArray ? " (Array)" : "") +
(IsList ? " (List)" : "") +
(IsDictionary ? " (Dictionary)" : "")

//(IsArrayOrListItem ? $" (Array or List item.{Index})" : "");
;
        }
    }
}
