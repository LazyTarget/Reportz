using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Reportz.Scripting.Attributes;
using Reportz.Scripting.Classes;
using Reportz.Scripting.Interfaces;

namespace Reportz.Scripting.Commands
{
    [ScriptElementAlias("alert")]
    public class AlertCommand : IExecutable, IScriptElement
    {
        private IScriptParser _instantiator;
        private string _message;

        public AlertCommand()
        {
            
        }
        
        public IExecutableResult Execute(IExecutableArgs args)
        {
            args = args ?? new ExecutableArgs { Scope = new VariableScope() };

            var message = _instantiator.EvaluateExpression(args.Scope, _message);
            Console.WriteLine(message);

            IExecutableResult result =new ExecutableResult
            {
                Result = message,
            };
            return result;
        }

        public void Configure(IScriptParser parser, XElement element)
        {
            _instantiator = parser;
            _message = element.Value;
        }
    }
}
