using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Reportz.Scripting.Attributes;
using Reportz.Scripting.Interfaces;

namespace Reportz.Scripting.Classes
{
    [ScriptElementAlias("script")]
    public class Script : IScript, IScriptElement
    {
        private IExecutableEnvironment _execute;
        private EventCollection _eventsCollection;

        public string Name { get; private set; }
        public IReadOnlyDictionary<string, IEvent> Events { get; private set; }

        public IExecutableResult Execute(IExecutableArgs args)
        {
            if (_execute == null)
                throw new InvalidOperationException("Execute logic hasn't been loaded");
            args = args ?? new ExecutableArgs { Scope = new VariableScope() };
            var result = _execute?.Execute(args);
            return result;
        }

        public void Configure(IScriptParser parser, XElement element)
        {
            Name = element.Attribute("name")?.Value;

            var executeElem = element.Element("execute") ?? element;
            if (executeElem != null)
            {
                var exe = new ExecutableEnvironment();
                exe.Configure(parser, executeElem);
                _execute = exe;
            }

            var eventsElem = element.Element("events");
            if (eventsElem != null)
            {
                var events = new EventCollection();
                events.Configure(parser, eventsElem);
                _eventsCollection = events;
            }
        }
    }
}
