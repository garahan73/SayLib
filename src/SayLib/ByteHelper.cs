using System;
using System.Collections.Generic;
using System.Text;

namespace Say32
{
    public class ByteHelper
    {
        public static Byte ToByte( object? value, bool force16bits = false ) => value switch
        {
            string s when force16bits || s.Trim().StartsWith("0x", "0X") => Convert.ToByte(s, 16),
            _ => Convert.ToByte(value)
        };
    }
}
