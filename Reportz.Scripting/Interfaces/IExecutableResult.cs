using System;
using System.Collections.Generic;
using System.Linq;

namespace Reportz.Scripting.Interfaces
{
    public interface IExecutableResult
    {
        IExecutableArgs Args { get; }
        object Result { get; }
    }
}
