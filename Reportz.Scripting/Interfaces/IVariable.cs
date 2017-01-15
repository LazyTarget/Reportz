using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reportz.Scripting.Interfaces
{
    public interface IVariable : IExecutable, ICloneable
    {
        string Key { get; }
        object Value { get; set; }
        Type Type { get; }
    }
}
