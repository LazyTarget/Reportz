using System;
using System.Collections.Generic;
using System.Linq;
using Lux.IO;
using Reportz.Scripting.Interfaces;

namespace Reportz.Scripting.Classes
{
    public class ScriptContext : IScriptContext
    {
        public ScriptContext()
        {
            ScriptScope = new ScriptScope();
            FileSystem = new FileSystem();
        }

        public IScriptScope ScriptScope { get; }
        public IFileSystem FileSystem { get; }
        // todo: IoC container?
    }
}
