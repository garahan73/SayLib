using System;
using System.Collections.Generic;
using System.Text;

namespace Say32
{
    public class AssertUtil
    {
        public static void ExpectAggregateException<T>(Action action) where T: Exception
        {
            try
            {
                action();
            }
            catch (AggregateException aex)
            {
                var ex = aex.StripAggregate();
                if (!(ex is T))
                    throw new Exception($"Expected exception type was {typeof(T).Name} but actual was {ex.GetType().Name}");

                return;
            }

            throw new Exception($"No exception thrown. Expected was {typeof(T).Name}");
        }
    }
}
