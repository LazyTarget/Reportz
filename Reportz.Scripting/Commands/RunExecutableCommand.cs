﻿using System;
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
        private IXInstantiator _instantiator;
        private XElement _element;
        private ArgCollection _argsCollection;
        private EventCollection _eventsCollection;

        public RunExecutableCommand()
        {
            
        }

        public string Key { get; private set; }

        //public IReadOnlyDictionary<string, IVariable> Arguments { get; private set; }

        public IReadOnlyDictionary<string, IEvent> Events { get; private set; }
        

        public IExecutableResult Execute(IExecutableArgs args)
        {
            IExecutableResult result = null;
            args = args ?? new ExecutableArgs { Scope = new VariableScope() };
            try
            {
                var variable = args.Scope?.GetVariable(Key);
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
                    //Arguments = Arguments?.Values?.Select(x => x.Value).ToArray(),
                    Arguments = new IVariable[0],
                    Scope = args.Scope?.CreateChild(),
                };
                
                var vars = _argsCollection;
                if (vars != null)
                {
                    foreach (var var in vars)
                    {
                        var exeArgs = new ExecutableArgs();
                        exeArgs.Scope = a.Scope;
                        exeArgs.Arguments = null;

                        var.Execute(exeArgs);
                        a.Arguments = a.Arguments.Concat(new[] { var }).ToArray();
                    }
                }

                result = executable.Execute(a);
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
                        Value = result,
                    };
                    args.Scope?.SetVariable(resultVar);
                    completeEvent.Execute(args);
                }
            }
        }

        public void Configure(IXInstantiator instantiator, XElement element)
        {
            _instantiator = instantiator;
            _element = element;

            Key = element.Attribute("key")?.Value;


            var argsElem = element.Element("arguments");
            if (argsElem != null)
            {
                var arg = new ArgCollection();
                arg.Configure(instantiator, argsElem);
                _argsCollection = arg;
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
