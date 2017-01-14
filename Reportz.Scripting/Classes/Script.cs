using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Reportz.Scripting.Interfaces;
using Reportz.Scripting.Xml;

namespace Reportz.Scripting.Classes
{
    public class Script : IScript, IXConfigurable
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

        public void Configure(IXInstantiator instantiator, XElement element)
        {
            Name = element.Attribute("name")?.Value;

            var executeElem = element.Element("execute");
            if (executeElem != null)
            {
                var exe = new ExecutableEnvironment();
                exe.Configure(instantiator, executeElem);
                _execute = exe;
            }

            var eventsElem = element.Element("events");
            if (eventsElem != null)
            {
                var events = new EventCollection();
                events.Configure(instantiator, eventsElem);
                _eventsCollection = events;
            }
        }
    }
}
