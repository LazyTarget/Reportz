using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using Reportz.Scripting.Attributes;
using Reportz.Scripting.Classes;
using Reportz.Scripting.Interfaces;

namespace Reportz.Scripting.Commands
{
    [ScriptElementAlias("execute-script")]
    public class ExecuteScriptCommand : IExecutable, IScriptElement
    {
        private IScriptParser _instantiator;
        private XElement _element;
        private ArgCollection _argsCollection;
        private EventCollection _eventsCollection;

        public ExecuteScriptCommand()
        {
            
        }

        public string ScriptName { get; private set; }
        

        public IExecutableResult Execute(IExecutableArgs args)
        {
            IExecutableResult result = null;
            args = args ?? new ExecutableArgs { Scope = new VariableScope() };
            try
            {
                var document = args?.Scope.GetScriptDocument();
                var scriptName = args?.Arguments?.FirstOrDefault(x => x.Key == "scriptName")?.Value?.ToString();
                scriptName     = scriptName ?? ScriptName;
                if (string.IsNullOrWhiteSpace(scriptName))
                    throw new Exception($"Invalid script name.");

                var script = document?.GetScript(scriptName);
                if (script == null)
                    throw new Exception($"Could not get script '{scriptName}'. ");

                var scriptArgs = new ExecutableArgs();
                scriptArgs.Scope = args.Scope.CreateChild();
                //scriptArgs.Arguments = _argsCollection?._vars?.Values?.ToArray();

                var scriptArgVars = _argsCollection?._vars?.Values?.ToArray();
                if (scriptArgVars != null)
                {
                    foreach (var scriptVar in scriptArgVars)
                    {
                        var r = scriptVar.Execute(scriptArgs);
                    }
                }

                result = script.Execute(scriptArgs);
                return result;
            }
            catch (Exception ex)
            {
                IEvent errorEvent;
                if (_eventsCollection._events.TryGetValue("error", out errorEvent) && errorEvent != null)
                {
                    var exceptionVar = new Variable
                    {
                        Key = "$$Exception",
                        Value = ex,
                    };
                    args.Scope?.SetVariable(exceptionVar);
                    errorEvent.Execute(args);
                }
                return result;

                // todo: implement 'catch' logic. catch="true" on <event key="error">. Or only if wrapped inside <try> <catch>
                // todo: implement test <throw> tag

                //throw;
            }
            finally
            {
                IEvent completeEvent;
                if (_eventsCollection._events.TryGetValue("complete", out completeEvent) && completeEvent != null)
                {
                    var resultVar = new Variable
                    {
                        Key = "$$Result",
                        Value = result?.Result,
                    };
                    args.Scope?.SetVariable(resultVar);
                    completeEvent.Execute(args);
                }
            }
        }

        public void Configure(IScriptParser parser, XElement element)
        {
            _instantiator = parser;
            _element = element;

            ScriptName = element?.Attribute("scriptName")?.Value ??
                         element?.Attribute("name")?.Value;

            var argsElem = element?.Element("arguments");
            if (argsElem != null)
            {
                var arg = new ArgCollection();
                arg.Configure(parser, argsElem);
                _argsCollection = arg;
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
