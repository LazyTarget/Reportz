using System;
using System.Collections.Generic;
using System.Linq;

namespace Reportz.Scripting.Interfaces
{
    public interface IScriptScope
    {
        IEnumerable<IScript> Scripts { get; }
        IScript GetScript(string scriptName);
        void AppendScript(IScript script);
    }
}
