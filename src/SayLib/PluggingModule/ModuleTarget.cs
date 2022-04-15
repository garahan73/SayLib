using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Say32.PluggingModule
{
    public interface IModuleTarget
    {
        ModuleSet Modules { get; }
    }

    public class ModuleSet
    {

        private readonly Dictionary<Type, object> _modules = new Dictionary<Type, object>();

        public T GetModule<T>() => (T)_modules[typeof(T)];

        public void Plug<T>(IPluggableModule<T> module)
        {
            _modules.Add(typeof(T), module);
            module.Modules = this;
        }
    }
}
