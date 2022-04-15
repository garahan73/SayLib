using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Say32.PluggingModule
{
    public interface IPluggableModule<T>
    {
        ModuleSet Modules { get; set; }
        
    }

}
