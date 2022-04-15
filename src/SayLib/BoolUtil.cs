using System;
using System.Collections.Generic;
using System.Text;

namespace Say32
{
    public static class SayBoolUtil
    {
        public static string ToStringWithBoolCorrection(this object obj)
        {
            if (obj is bool || obj is Boolean)
                return obj.ToString().ToLower();
            else
                return obj.ToString();
        }
    }
}
