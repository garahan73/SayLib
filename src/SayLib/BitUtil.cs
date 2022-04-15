using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.CompilerServices;

namespace Say32
{
    public struct Bit
    {
        public bool Value { get; set; }

        public static implicit operator Bit( bool value ) => new Bit { Value = value };
        public static implicit operator Bit( int value ) => new Bit { Value = value != 0 };
        public static implicit operator Bit( short value ) => new Bit { Value = value != 0 };
        public static implicit operator Bit( string value ) => Parse(value);

        public static implicit operator bool( Bit bit ) => bit.Value;
        public static implicit operator int( Bit bit ) => bit.Value ? 1 : 0;
        public static implicit operator short( Bit bit ) => (short)(int)bit;

        public static byte GetByteMask( int offset ) => (byte)(1 << (7 - offset));

        public override string ToString()
        {
            return ((int)this).ToString();
        }

        public static Bit Parse(string text)
        {
            try
            {
                return (Bit)Convert.ToBoolean(text);
            }
            catch 
            {
                try
                {
                    return (Bit)Convert.ToInt32(text);
                }
                catch (Exception)
                {
                    throw new Exception($"Can't convert text('{text}') to bit");
                }
            }
        }

        public override bool Equals( object obj )
        {
            if (obj == null) return false;

            var bit = obj switch
            {
                Bit b => b,
                bool bl => (Bit)bl,
                int num => (Bit)num,
                string text => (Bit)text,
                _ => (Bit)!Value,
            };
            return  bit.Value == Value; 
        }
        public override int GetHashCode() => Value.GetHashCode();

        public static bool operator==( Bit bit, object value )
        {
            return bit.Equals(value);
        }

        public static bool operator !=( Bit bit, object value )
        {
            return !bit.Equals(value);
        }

    }

    public static class BitUtil
    {
        //public static BitArray ToBitArray( this IEnumerable<byte> bytes )
        //{
        //    return new BitArray(ToBits(bytes).Select(b=> (bool)b).ToArray() );
        //}

        public static List<Bit> ToBits( this IEnumerable<byte> bytes )
        {
            var bits = new List<Bit>(8 * bytes.Count());
            bytes.ForEach(b =>
            {
                for (int bitNumber = 0; bitNumber < 8; bitNumber++)
                {
                    var bit = (b & (1 << 7 - bitNumber)) != 0;
                    bits.Add(bit);
                }
            });

            return bits;
        }

        public static List<byte> ToBytes( this IEnumerable<Bit> bits )
        {
            var bitList = bits.ToList();
            var bytes = new List<byte>();

            byte @byte = 0;

            for (int bitNumber = 0, bitIndex = 0; bitIndex < bitList.Count; bitNumber++, bitIndex++)
            {
                if (bitNumber == 8)
                    bitNumber = 0;

                if (bitNumber == 0)
                {
                    @byte = 0;
                    bytes.Add(@byte);
                }

                var bit = bitList[bitIndex];
                var bitFilter = (byte)(bit << 7 - bitNumber);
                @byte = (byte)(@byte | bitFilter);
                bytes[bytes.Count - 1] = @byte;
            }

            return bytes;
        }

        public static List<byte> ToBytes( this BitArray bitArray )
        {
            var bytes = new List<byte>();

            byte @byte = 0;

            for (int bitNumber = 0, bitIndex = 0; bitIndex < bitArray.Count; bitNumber++, bitIndex++)
            {
                if (bitNumber == 8)
                    bitNumber = 0;

                if (bitNumber == 0)
                {
                    @byte = 0;
                    bytes.Add(@byte);
                }

                var bit = bitArray[bitIndex];
                var bitFilter = (byte)( (bit ? 1: 0) << 7 - bitNumber);
                @byte = (byte)(@byte | bitFilter);
                bytes[bytes.Count - 1] = @byte;
            }

            return bytes;
        }

        public static Bit Get( this byte @byte, int offset )
        {
            var mask = Bit.GetByteMask(offset);
            return (byte)(@byte & mask);
        }

        public static byte Set( this byte @byte, int offset, Bit bit )
        {
            var mask = Bit.GetByteMask(offset);
            return (byte)((bool)bit ? @byte | mask : @byte & ~mask);
        }

        public static byte Toggle( this byte @byte, int offset )
        {
            var mask = Bit.GetByteMask(offset);
            return (byte)(@byte ^ mask);
        }


        public static Bit GetBit( this byte[] bytes, int offset )
        {
            var byteIndex = offset / 8;
            var bitOffset = offset % 8;
            return bytes[byteIndex].Get(bitOffset);
        }

        public static byte[] SetBit( this byte[] bytes, int offset, Bit bit )
        {
            var byteIndex = offset / 8;
            var bitOffset = offset % 8;

            bytes[byteIndex] = bytes[byteIndex].Set(bitOffset, bit);

            return bytes;
        }

    }
}
