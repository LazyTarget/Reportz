using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Reportz.Scripting.Classes;
using Reportz.Scripting.Interfaces;
using Reportz.Scripting.Xml;

namespace Reportz.Scripting.Commands
{
    public class RunExecutableCommand : IExecutable, IXConfigurable, IHasEvents
    {
        public string Key { get; private set; }

        public IReadOnlyDictionary<string, IVariable> Arguments { get; private set; }

        public IReadOnlyDictionary<string, IEvent> Events { get; private set; }
        
        public IExecutableResult Execute(IExecutableArgs args)
        {
            args = args ?? new ExecutableArgs { Scope = new VariableScope() };

            var variable = args?.Scope?.GetVariable(Key);
            if (variable?.Value == null)
            {
                throw new InvalidOperationException("Invalid variable value.");
            }
            var executable = variable.Value as IExecutable;
            if (executable == null)
            {
                throw new InvalidOperationException("Variable is not an executable.");
            }

            var a = new ExecutableArgs
            {
                Arguments = Arguments?.Values?.Select(x => x.Value).ToArray(),
                Scope = args?.Scope?.CreateChild(),
            };
            var result = executable.Execute(a);
            return result;
        }

        public void Configure(IXInstantiator instantiator, XElement element)
        {
            Key = element.Attribute("key")?.Value;

            // todo: populate Arguments

            // todo: populate Events

        }
    }
}
