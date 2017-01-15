using System;
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
    public class AlertCommand : IExecutable, IXConfigurable
    {
        private IXInstantiator _instantiator;
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

        public void Configure(IXInstantiator instantiator, XElement element)
        {
            _instantiator = instantiator;
            _message = element.Value;
        }
    }
}
