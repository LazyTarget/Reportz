﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Reportz.Scripting.Classes;

namespace Reportz.Scripting.Interfaces
{
    public interface IExecutableArgs
    {
        VariableScope Scope { get; }
        object[] Arguments { get; }
    }
}
