using System;
using System.Collections.Generic;
using System.Text;

namespace Say32
{
    public static class IntUtil
    {
        public static bool IsOdd(this int value) => value % 2 == 1;
        public static bool IsEven(this int value) => value % 2 == 0;
    }
}
