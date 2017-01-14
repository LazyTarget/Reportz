using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Reportz.Scripting.Interfaces;

namespace Reportz.Scripting.Classes
{
    public class ExecutableArgs : IExecutableArgs
    {
        public VariableScope Scope { get; set; }
        public object[] Arguments { get; set; }
    }
}
