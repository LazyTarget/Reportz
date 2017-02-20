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
    [ScriptElementAlias("invoke-method")]
    public class InvokeMethodCommand : IExecutable, IScriptElement, IHasEvents
    {
        private IScriptParser _instantiator;
        private XElement _element;
        private ArgCollection _argsCollection;
        private EventCollection _eventsCollection;

        public InvokeMethodCommand()
        {
            
        }
        
        //public IReadOnlyDictionary<string, IVariable> Arguments { get; private set; }

        public IReadOnlyDictionary<string, IEvent> Events { get; private set; }
        

        public IExecutableResult Execute(IExecutableArgs args)
        {
            IExecutableResult result = null;
            args = args ?? new ExecutableArgs { Scope = new VariableScope() };
            try
            {
                var fullArgsList = new List<IVariable>();
                if (args.Arguments != null)
                    fullArgsList.AddRange(args.Arguments);
                if (_argsCollection != null)
                {
                    var varArgs = new ExecutableArgs();
                    varArgs.Scope = args.Scope;
                    //exeArgs.Scope = args.Scope.CreateChild();
                    varArgs.Arguments = null;

                    foreach (var var in _argsCollection)
                    {
                        var.Execute(varArgs);
                        fullArgsList.Add(var);
                    }
                }

                Type type;
                var instance = fullArgsList?.FirstOrDefault(x => x.Key == "instance")?.Value;
                var typeTmp = fullArgsList?.FirstOrDefault(x => x.Key == "type")?.Value ??
                              fullArgsList?.FirstOrDefault(x => x.Key == "typeName")?.Value;
                if (typeTmp is Type)
                {
                    type = (Type) typeTmp;
                }
                else if (!string.IsNullOrWhiteSpace(typeTmp?.ToString()))
                {
                    var typeResolved = _instantiator.TryResolveType(typeTmp.ToString(), out type);
                    if (!typeResolved || type == null)
                        throw new Exception($"Type '{typeTmp}' not found");
                }
                else
                {
                    // Get from instance
                    type = instance?.GetType();
                    if (type == null)
                        throw new Exception($"Could not resolve type from instance");
                }


                string methodName;
                MethodInfo method;
                var asStatic = instance == null;
                var methodArgsTmp = (fullArgsList?.FirstOrDefault(x => x.Key == "arguments")?.Value as IEnumerable<IVariable>)?.ToList();
                if (methodArgsTmp != null)
                {
                    var varArgs = new ExecutableArgs();
                    varArgs.Scope = args.Scope;
                    //exeArgs.Scope = args.Scope.CreateChild();
                    varArgs.Arguments = null;

                    methodArgsTmp.ForEach(x =>
                    {
                        x.Execute(varArgs);
                    });
                }
                var methodArgs = methodArgsTmp?.Select(x => x.Value).ToArray() ?? new object[0];
                var methodArgTypes = methodArgsTmp?.Select(x => x.Type).ToArray() ?? new Type[0];

                var methodTmp = fullArgsList?.FirstOrDefault(x => x.Key == "method")?.Value;
                if (methodTmp is MethodInfo)
                {
                    method = (MethodInfo)methodTmp;
                    methodName = method.Name;
                }
                else
                {
                    methodName = methodTmp?.ToString();
                    if (string.IsNullOrWhiteSpace(methodName))
                        throw new InvalidOperationException($"Invalid method name");

                    var bindingFlagsTmp = fullArgsList?.FirstOrDefault(x => x.Key == "bindingFlags")?.Value;
                    BindingFlags bindingFlags;
                    if (bindingFlagsTmp is BindingFlags)
                        bindingFlags = (BindingFlags)bindingFlagsTmp;
                    else
                    {
                        if (asStatic)
                            bindingFlags = BindingFlags.Public | BindingFlags.Static | BindingFlags.InvokeMethod | BindingFlags.OptionalParamBinding;
                        else
                            bindingFlags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.InvokeMethod | BindingFlags.OptionalParamBinding;
                    }

                    //method = type.GetMethod(methodName, bindingFlags);
                    method = type.GetMethod(methodName, bindingFlags, null, methodArgTypes, null);
                    
                    var methodParams = method.GetParameters().ToArray();
                    if (methodParams.Any(x => x.IsOptional))
                    {
                        for (var i = 0; i < methodParams.Length; i++)
                        {
                            if (methodArgs.Length >= methodParams.Length)
                                continue;
                            var isAssigned = methodArgs.Length > i;
                            if (isAssigned)
                                continue;
                            var param = methodParams[i];
                            if (param.IsOptional)
                            {
                                var l = methodArgs.ToList();
                                l.Insert(i, param.DefaultValue);
                                methodArgs = l.ToArray();
                            }
                        }
                    }
                }

                var methodResult = method.Invoke(instance, methodArgs);

                result = args.CreateResult(methodResult);
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

            var argsElem = element.Element("arguments");
            if (argsElem != null)
            {
                var arg = new ArgCollection();
                arg.Configure(parser, argsElem);
                _argsCollection = arg;
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
