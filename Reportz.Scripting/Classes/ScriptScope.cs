using System;
using System.Collections.Generic;
using System.Linq;
using Reportz.Scripting.Interfaces;

namespace Reportz.Scripting.Classes
{
    public class ScriptScope : IScriptScope
    {
        private readonly IList<IScript> _scripts;

        public ScriptScope()
        {
            _scripts = new List<IScript>();
        }

        public IEnumerable<IScript> Scripts => _scripts.AsEnumerable();

        public IScript GetScript(string scriptName)
        {
            var script = _scripts.FirstOrDefault(x => x.Name == scriptName);
            return script;
        }

        public void AppendScript(IScript script)
        {
            var exists = _scripts.Contains(script);
            if (!exists)
                _scripts.Add(script);
        }
    }
}
