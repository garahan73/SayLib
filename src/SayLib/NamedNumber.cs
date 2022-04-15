using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Say32
{
    public struct NamedNumber<TNumber> 
        where TNumber : struct
    {

        public NamedNumber( TNumber number, string? name = null )
        {
            Number = number;
            Name = name;
        }

        public TNumber Number { get; set; }
        public string? Name { get; set; }
        public string NameOrNumber => Name ?? Number.ToString();

        public override string ToString() => Number.ToString();

        public static implicit operator NamedNumber<TNumber>( TNumber number ) => new NamedNumber<TNumber>(number);
        public static implicit operator NamedNumber<TNumber>( (TNumber id, string name) t ) => new NamedNumber<TNumber>(t.id, t.name);

        public static implicit operator TNumber( NamedNumber<TNumber> id ) => id.Number;
        public static implicit operator string?( NamedNumber<TNumber> id ) => id.Name;

        public override bool Equals( object obj ) => obj switch
        {
            null => false,
            NamedNumber<TNumber> id => Equals(Number, id.Number),
            TNumber id => Equals(Number, id),
            _ => Try.Run(() => Convert.ChangeType(obj, typeof(TNumber)), out var id, out _) ?
                Equals(Number, id) : false,
        };

        public static bool operator ==( NamedNumber<TNumber> id1, object id2 ) => Equals(id1, id2);
        public static bool operator !=( NamedNumber<TNumber> id1, object id2 ) => !(id1 == id2);


        public override int GetHashCode() => Number.GetHashCode();
    }


}

