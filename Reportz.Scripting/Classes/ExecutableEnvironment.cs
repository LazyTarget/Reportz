using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Reportz.Scripting.Interfaces;

namespace Reportz.Scripting.Classes
{
    public class ExecutableEnvironment : IExecutableEnvironment, IScriptElement
    {
#if DEBUG
        public readonly IList<object> _elements = new List<object>();
#else
        private readonly IList<object> _elements = new List<object>();
#endif

        public IExecutableResult Execute(IExecutableArgs args)
        {
            IVariable resultVar = null;

            IExecutableResult result = null;
            var executables = _elements.OfType<IExecutable>().ToList();
            foreach (var executable in executables)
            {
                var a = new ExecutableArgs
                {
                    Scope = args.Scope.CreateChild(),
                    //Arguments = args.Arguments,
                    Arguments = null,
                };
                var r = executable.Execute(a);


                if (executables.Count == 1)
                {
                    result = r;
                }
                else
                {
                    //var tmp = args.Scope.GetVariable("$$Result");
                    var tmp = a.Scope.GetVariable("$$Result");
                    if (tmp != null)
                        resultVar = tmp;
                }
            }

            
            if (resultVar != null)
            {
                result = new ExecutableResult
                {
                    Result = resultVar.Value,
                };
            }

            return result;
        }

        public void Configure(IScriptParser parser, XElement element)
        {
            var children = element.Elements();
            foreach (var child in children)
            {
                var obj = parser.InstantiateElement(child);
                _elements.Add(obj);
            }
        }
    }
}
