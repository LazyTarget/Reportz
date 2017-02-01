using System;
using System.Collections.Generic;
using System.Linq;
using Reportz.Scripting.Interfaces;

namespace Reportz.Scripting.Classes
{
    public class ScriptContext : IScriptContext
    {
        public ScriptContext()
        {
            ScriptScope = new ScriptScope();
        }

        public IScriptScope ScriptScope { get; private set; }
    }
}
