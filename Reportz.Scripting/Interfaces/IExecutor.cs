using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reportz.Scripting.Interfaces
{
    public interface IExecutor
    {
        IExecutableResult Execute(IExecutableArgs args);
    }
}
