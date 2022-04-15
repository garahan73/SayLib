using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Say32
{
    public static class UiUtil
    {
        // Extension method.
        public static void SynchronizedInvoke( this ISynchronizeInvoke sync, Action action )
        {
            // If the invoke is not required, then invoke here and get out.
            if (!sync.InvokeRequired)
            {
                // Execute action.
                action();

                // Get out.
                return;
            }
            else // need invokation
                // Marshal to the required context.
                sync.Invoke(action, new object[] { });
        }


        public static ControlAwaiter GetAwaiter( this ISynchronizeInvoke control )
        {
            return new ControlAwaiter(control);
        }

        public struct ControlAwaiter : INotifyCompletion
        {
            private readonly ISynchronizeInvoke m_control;

            public ControlAwaiter( ISynchronizeInvoke control )
            {
                m_control = control;
            }

            public bool IsCompleted
            {
                get { return !m_control.InvokeRequired; }
            }

            public void OnCompleted( Action continuation )
            {
                m_control.Invoke(continuation, new object[] { });
            }

            public void GetResult() { }
        }
    }
}
