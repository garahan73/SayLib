using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Say32
{
    public static class SayCollectionUtil
    {
        public static bool AreEqual<T>(IEnumerable<T> a, IEnumerable<T> b)
        {
            if (CompareNull(a, b, out bool areEqual))
                return areEqual;

            var count = a.Count();
            if (count != b.Count()) return false;

            var al = new List<T>(a);
            var bl = new List<T>(b);

            for (int i = 0; i < count; i++)
            {
                var ac = al[i];
                var bc = bl[i];

                if (CompareNull(ac, bc, out areEqual))
                    return areEqual;

                if (!Equals(ac, bc))
                    return false;
            }

            return true;
        }

        public static bool CompareNull(object a, object b, out bool areEqual)
        {
            if (a == null)
            {
                areEqual= b == null;
                return true;
            }
            // a != null
            if (b == null)
            {
                areEqual = false;
                return true;
            }

            areEqual = false;
            return false;
        }

        public static bool CompareNull<T>(T a, T b, out bool areEqual)
        {
            if (a == null)
            {
                areEqual = b == null;
                return true;
            }
            // a != null
            if (b == null)
            {
                areEqual = false;
                return true;
            }

            areEqual = false;
            return false;
        }

    }
}
