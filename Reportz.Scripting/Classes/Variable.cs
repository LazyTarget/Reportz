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
    //public class Variable : IVariable
    //{
    //    private Type _explicitType;

    //    public string Key { get; set; }
    //    public object Value { get; set; }
    //    public Type Type => _explicitType ?? Value?.GetType();
    //}

    public class Variable : IVariable, IExecutable, IXConfigurable
    {
        private Type _explicitType;

        public string Key { get; set; }
        public object Value { get; set; }
        public Type Type => _explicitType ?? Value?.GetType();
        
        public IExecutableResult Execute(IExecutableArgs args)
        {
            args.Scope?.Parent?.SetVariable(this);

            var success = args.Scope?.Parent?.GetVariable(Key) != null &&
                          args.Scope?.Parent?.GetVariable(Key).Value == Value;
            var result = new ExecutableResult
            {
                Result = success,
            };
            return result;
        }

        public void Configure(IXInstantiator instantiator, XElement element)
        {
            Key = element.Attribute("key")?.Value;
            Value = element.Attribute("value")?.Value;

            if (element.Attribute("value") == null && !element.HasElements)
            {
                Value = element.Value;
            }
        }
    }
}
