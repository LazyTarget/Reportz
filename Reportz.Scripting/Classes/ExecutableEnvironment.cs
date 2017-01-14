﻿using System;
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
        public readonly IList<object> _elements = new List<object>();
#else
        private readonly IList<object> _elements = new List<object>();
#endif

        public IExecutableResult Execute(IExecutableArgs args)
        {
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
                _elements.Add(obj);
            }
        }
    }
}
