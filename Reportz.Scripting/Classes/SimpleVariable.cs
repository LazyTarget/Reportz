using System;
using System.Collections.Generic;
using System.Linq;
using Reportz.Scripting.Interfaces;

namespace Reportz.Scripting.Classes
{
    public class SimpleVariable : IVariable, ICloneable
    {
        private SimpleVariable(string key)
        {
            Key = key;
        }

        public SimpleVariable(string key, object value)
            : this(key)
        {
            Value = value;
        }


        public string Key { get; private set; }

        public object Value { get; set; }
        public Type Type => Value?.GetType();
        public bool Instantiated => true;

#if DEBUG
        public bool Executed { get; private set; }
#else
        private bool Executed { get; private set; }
#endif

        public IExecutableResult Execute(IExecutableArgs args)
        {
            int depth;
            var rootRaw = args.Arguments?.FirstOrDefault(x => x.Key == "depth")?.Value?.ToString();
            int.TryParse(rootRaw, out depth);
            if (rootRaw == "root")
            {
                args.Scope?.Root?.SetVariable(this);
            }
            else
            {
                // implement numeric navigation??
                args.Scope?.Parent?.SetVariable(this);
            }
            

            var success = args.Scope?.Parent?.GetVariable(Key) != null &&
                          args.Scope?.Parent?.GetVariable(Key).Value == Value;
            var result = new ExecutableResult
            {
                Result = success,
            };
            Executed = true;
            return result;
        }

        public object Clone()
        {
            var result = new SimpleVariable(Key, Value);
            return result;
        }
    }
}
