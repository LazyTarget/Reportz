using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Reportz.Scripting.Attributes;
using Reportz.Scripting.Interfaces;

namespace Reportz.Scripting.Classes
{
    [ScriptElementAlias("document")]
    public class ScriptDocument : IScriptDocument, IScriptElement
    {
        private IScriptParser _parser;
        private XElement _element;
        private IExecutableEnvironment _execute;
        private EventCollection _eventsCollection;
        private readonly IList<IScript> _scripts;

        public ScriptDocument()
        {
            _scripts = new List<IScript>();
        }

        public ScriptDocument(IEnumerable<IScript> scripts)
            : this()
        {
            if (scripts == null)
                throw new ArgumentNullException(nameof(scripts));
            foreach (var script in scripts)
            {
                _scripts.Add(script);
            }
        }

        public string Name { get; private set; }
        public IReadOnlyDictionary<string, IEvent> Events { get; private set; }

        public IEnumerable<IScript> Scripts => _scripts;


        public IScript GetScript(string scriptName)
        {
            var script = _scripts?.FirstOrDefault(x => x.Name == scriptName);
            return script;
        }

        public IExecutableResult Execute(IExecutableArgs args)
        {
            if (_execute == null)
                throw new InvalidOperationException("Execute logic hasn't been loaded");
            args = args ?? new ExecutableArgs { Scope = new VariableScope() };
            
            // Global variables
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
                _scripts.Clear();

                var scriptElements = scriptsElem.Elements("script");
                foreach (var scriptElement in scriptElements)
                {
                    var obj = parser.InstantiateElement(scriptElement);
                    var script = obj as IScript;
                    if (script == null)
                        continue;
                    _scripts.Add(script);
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
