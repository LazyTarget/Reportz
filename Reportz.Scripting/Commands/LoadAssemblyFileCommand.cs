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
using Lux.IO;

namespace Reportz.Scripting.Commands
{
    [ScriptElementAlias("using")]
    [ScriptElementAlias("load-assembly-file")]
    public class LoadAssemblyFileCommand : IExecutable, IScriptElement
    {
        private IScriptParser _parser;
        private XElement _element;
        private ArgCollection _argsCollection;
        private EventCollection _eventsCollection;

        public LoadAssemblyFileCommand()
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

                // Load assembly
                byte[] rawAssembly;
                byte[] rawSymbolStore = null;
                using (var fileStream = ctx.FileSystem.OpenFile(FilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    rawAssembly = ReadToEnd(fileStream);
                }

                // Load symbols
#if DEBUG
                var fileName = Path.GetFileName(FilePath);
                if (fileName.Contains("."))
                {
                    var directory = PathHelper.GetParentOrDefault(FilePath);
                    var symbolFileName = fileName.Substring(0, fileName.LastIndexOf('.')) + ".pdb";
                    var symbolFilePath = directory != null
                        ? PathHelper.Combine(directory, symbolFileName)
                        : symbolFileName;
                    if (ctx.FileSystem.FileExists(symbolFilePath))
                    {
                        using (var fileStream = ctx.FileSystem.OpenFile(symbolFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                        {
                            rawSymbolStore = ReadToEnd(fileStream);
                        }
                    }
                }
#endif


                Assembly assembly = null;
//#if DEBUG
//                //assembly = Assembly.Load(rawAssembly);
//                assembly = Assembly.Load(rawAssembly, rawSymbolStore);
//                //assembly = AppDomain.CurrentDomain.Load(rawAssembly);
//                //assembly = AppDomain.CurrentDomain.Load(rawAssembly, rawSymbolStore);
//#else
                var assName = AssemblyName.GetAssemblyName(FilePath);
                assembly = Assembly.Load(assName);
//#endif

                object obj = assembly;
                result = args.CreateResult(obj);
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


        private byte[] ReadToEnd(Stream stream)
        {
            long originalPosition = 0;
            if (stream.CanSeek)
            {
                originalPosition = stream.Position;
                stream.Position = 0;
            }

            try
            {
                byte[] readBuffer = new byte[4096];

                int totalBytesRead = 0;
                int bytesRead;

                while ((bytesRead = stream.Read(readBuffer, totalBytesRead, readBuffer.Length - totalBytesRead)) > 0)
                {
                    totalBytesRead += bytesRead;

                    if (totalBytesRead == readBuffer.Length)
                    {
                        int nextByte = stream.ReadByte();
                        if (nextByte != -1)
                        {
                            byte[] temp = new byte[readBuffer.Length * 2];
                            Buffer.BlockCopy(readBuffer, 0, temp, 0, readBuffer.Length);
                            Buffer.SetByte(temp, totalBytesRead, (byte)nextByte);
                            readBuffer = temp;
                            totalBytesRead++;
                        }
                    }
                }

                byte[] buffer = readBuffer;
                if (readBuffer.Length != totalBytesRead)
                {
                    buffer = new byte[totalBytesRead];
                    Buffer.BlockCopy(readBuffer, 0, buffer, 0, totalBytesRead);
                }
                return buffer;
            }
            finally
            {
                if (stream.CanSeek)
                {
                    stream.Position = originalPosition;
                }
            }
        }

    }
}
