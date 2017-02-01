using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Reportz.Scripting.Attributes;
using Reportz.Scripting.Interfaces;

namespace Reportz.Scripting.Classes
{
    [ScriptElementAlias("document")]
    public class ScriptDocument : IScriptDocument, IScriptElement, IHasScriptContext
    {
        private IScriptParser _parser;
        private XElement _element;
        private IExecutableEnvironment _execute;
        private EventCollection _eventsCollection;

        public ScriptDocument()
        {

        }

        public string Name { get; private set; }
        public IReadOnlyDictionary<string, IEvent> Events { get; private set; }
        
        public IScriptContext Context { get; set; }
        

        public IExecutableResult Execute(IExecutableArgs args)
        {
            if (_execute == null)
                throw new InvalidOperationException("Execute logic hasn't been loaded");
            args = args ?? new ExecutableArgs { Scope = new VariableScope() };
            
            // Global variables
            args.Scope.CreateLazyVariable("$$Context", () => Context);
            args.Scope.CreateSimpleVariable("$$Document", this);
            args.Scope.CreateSimpleVariable("$$Parser", _parser);

            var result = _execute?.Execute(args);
            return result;
        }

        public void Configure(IScriptParser parser, XElement element)
        {
            _parser = parser;
            _element = element;

            Name = element?.Attribute("name")?.Value;

            var scriptsElem = element?.Element("scripts") ?? element;
            if (scriptsElem != null)
            {
                var scriptElements = scriptsElem.Elements("script");
                foreach (var scriptElement in scriptElements)
                {
                    var obj = parser.InstantiateElement(scriptElement);
                    var script = obj as IScript;
                    if (script == null)
                        continue;
                    Context?.ScriptScope?.AppendScript(script);
                }
            }

            var executeElem = element?.Element("execute");
            if (executeElem != null)
            {
                var exe = new ExecutableEnvironment();
                exe.Configure(parser, executeElem);
                _execute = exe;
            }

            var eventsElem = element?.Element("events");
            if (eventsElem != null)
            {
                var events = new EventCollection();
                events.Configure(parser, eventsElem);
                _eventsCollection = events;
            }
        }
    }
}
