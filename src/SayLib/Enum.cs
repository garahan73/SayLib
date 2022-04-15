using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Say32
{
    public static class EnumExtensions
    {
        public static long GetLongValue<T>(this T source) where T : Enum//IConvertible//enum
        {
            return (long)Convert.ChangeType(source, typeof(long));
        }


        public static int Count<T>(this T soure) where T : Enum // IConvertible//enum
        {
            if (!typeof(T).IsEnum)
                throw new ArgumentException("T must be an enumerated type");

            return Enum.GetNames(typeof(T)).Length;
        }
    }

    public abstract class SayEnum<TCode, TEnum> where TEnum : SayEnum<TCode, TEnum>        
    {
        public TCode _Code_ { get; }
        public string _Name_ { get; }

        public static SayEnumValues<TCode, TEnum> Values { get; } = new SayEnumValues<TCode, TEnum>();

        protected SayEnum(string name, TCode code)
        {
            _Code_ = code;
            _Name_ = name;

            Values.Register((TEnum)this, code, name);
        }

        //protected static Func<string, TCode, TEnum> Create;

        public override string ToString() => _Name_;

        public override int GetHashCode() => _Code_?.GetHashCode() ?? 0;
        public override bool Equals(object obj) => obj is TEnum sayEnum && Equals(_Code_, sayEnum._Code_);

        

        public static explicit operator TCode(SayEnum<TCode, TEnum> @enum) => @enum._Code_;
        public static explicit operator SayEnum<TCode, TEnum>(TCode code) => Values[code];

        //public static implicit operator SayEnum<TCode, TEnum>((string name, TCode code) t) => Create(t.name, t.code);

        public void Deconstruct(out TCode code, out string name)
        {
            code = _Code_; name = _Name_;
        }
    }

    public class SayEnumValues<TCode, TEnum>
    {
        private Dictionary<TCode, TEnum> CodeMap { get; } = new Dictionary<TCode, TEnum>();
        private Dictionary<string, TEnum> NameMap { get; } = new Dictionary<string, TEnum>();

        internal void Register( TEnum sayEnum, TCode code, string name ) 
        {
            CodeMap.Add(code, sayEnum);
            NameMap.Add(name, sayEnum);
        }

        public bool IsValidCode( TCode code ) => CodeMap.ContainsKey(code);
        public bool IsValidName( string name ) => NameMap.ContainsKey(name);

        public TCode[] Codes => CodeMap.Keys.ToArray();
        public string[] Names => NameMap.Keys.ToArray();

        public TEnum this[TCode code] => CodeMap[code];
        public TEnum this[string name] => NameMap[name];

    }
}
