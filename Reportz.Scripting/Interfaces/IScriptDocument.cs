using System;
using System.Collections.Generic;
using System.Linq;

namespace Reportz.Scripting.Interfaces
{
    public interface IScriptDocument : IExecutableEnvironment, IHasEvents
    {
        string Name { get; }
        IEnumerable<IScript> Scripts { get; }
        IScript GetScript(string scriptName);
    }
}
