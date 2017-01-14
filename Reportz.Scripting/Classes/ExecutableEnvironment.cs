using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Reportz.Scripting.Interfaces;
using Reportz.Scripting.Xml;

namespace Reportz.Scripting.Classes
{
    public class ExecutableEnvironment : IExecutableEnvironment, IXConfigurable
    {
#if DEBUG
        public readonly IList<IExecutable> _list = new List<IExecutable>();
#else
        private readonly IList<IExecutable> _list = new List<IExecutable>();
#endif

        public IExecutableResult Execute(IExecutableArgs args)
        {
            IExecutableResult result = null;
            foreach (var executable in _list)
            {
                var a = new ExecutableArgs
                {
                    Scope = args.Scope.CreateChild(),
                    //Arguments = args.Arguments,
                    Arguments = null,
                };
                var r = executable.Execute(a);
                if (_list.Count == 1)
                    result = r;
            }
            return result;
        }

        public void Configure(IXInstantiator instantiator, XElement element)
        {
            var children = element.Elements();
            foreach (var child in children)
            {
                var obj = instantiator.InstantiateElement(child);
                var executable = obj as IExecutable;
                _list.Add(executable);
            }
        }
    }
}
