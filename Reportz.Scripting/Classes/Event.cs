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
    public class Event : IEvent, IXConfigurable
    {
        private IExecutableEnvironment _execute;


        public string Key { get; private set; }

        public IExecutableResult Execute(IExecutableArgs args)
        {
            if (_execute == null)
                throw new InvalidOperationException("Execute logic hasn't been loaded");
            var result = _execute?.Execute(args);
            return result;
        }

        public void Configure(IXInstantiator instantiator, XElement element)
        {
            Key = element.Attribute("key")?.Value;

            var executeElem = element.Element("execute") ?? element;
            if (executeElem != null)
            {
                var exe = new ExecutableEnvironment();
                exe.Configure(instantiator, executeElem);
                _execute = exe;
            }
        }
    }
}
