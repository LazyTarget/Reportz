using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Linq;
using Reportz.Scripting.Attributes;
using Reportz.Scripting.Classes;
using Reportz.Scripting.Interfaces;

namespace Reportz.Scripting.Commands
{
    [ScriptElementAlias("include")]
    [ScriptElementAlias("load-script-file")]
    public class LoadScriptFileCommand : IExecutable, IScriptElement
    {
        private IScriptParser _parser;
        private XElement _element;
        private ArgCollection _argsCollection;
        private EventCollection _eventsCollection;

        public LoadScriptFileCommand()
        {
            _eventsCollection = new EventCollection();
        }

        public string FilePath { get; private set; }
        

        public IExecutableResult Execute(IExecutableArgs args)
        {
            IExecutableResult result = null;
            args = args ?? new ExecutableArgs { Scope = new VariableScope() };
            try
            {
                if (string.IsNullOrWhiteSpace(FilePath))
                    throw new Exception($"Invalid script path.");
                
                var ctx = args?.Scope.GetScriptContext();
                var fileExists = ctx.FileSystem.FileExists(FilePath);
                if (!fileExists)
                    throw new Exception($"Could not find script at path: '{FilePath}'. ");

                string scriptContents;
                Encoding encoding = Encoding.UTF8;
                using (var fileStream = ctx.FileSystem.OpenFile(FilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (var streamReader = new StreamReader(fileStream, encoding))
                {
                    scriptContents = streamReader.ReadToEnd();
                }

                var loadedScriptDoc = _parser.ParseDocument(scriptContents);
                var loadedCtx = ((IHasScriptContext) loadedScriptDoc).Context;
                if (loadedCtx != ctx)
                {
                    var loadedScripts = loadedCtx.ScriptScope.Scripts.ToArray();
                    foreach (var loadedScript in loadedScripts)
                    {
                        ctx.ScriptScope.AppendScript(loadedScript);
                    }
                }

                // Run ExecutionEnvironment
                var a = new ExecutableArgs
                {
                    Scope = args.Scope.CreateChild(),
                    Arguments = null,
                };
                var r = loadedScriptDoc.Execute(a);

                object res = loadedScriptDoc;
                result = args.CreateResult(res);
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
            _parser = parser;
            _element = element;

            FilePath = element?.Attribute("path")?.Value;

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
