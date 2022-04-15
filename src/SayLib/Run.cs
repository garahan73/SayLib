using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Say32
{
    public static class Run
    {
        public static void Safely( this Action action )
        {
            try
            {
                action();
            }
            catch { }
        }

        public static T Safely<T>( this Func<T> func )
        {
            try
            {
                return func();
            }
            catch
            {
#pragma warning disable CS8603 // 가능한 null 참조 반환입니다.
                return default;
#pragma warning restore CS8603 // 가능한 null 참조 반환입니다.
            }
        }
    }
}
