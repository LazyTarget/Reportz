using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Reportz.Scripting.Interfaces;

namespace Reportz.Scripting.Classes
{
    public class LazyVariable : IVariable, ICloneable
    {
        private Lazy<object> _lazy; 


        private LazyVariable(string key)
        {
            Key = key;
        }

        private LazyVariable(string key, Lazy<object> lazy)
            : this(key)
        {
            _lazy = lazy;
        }

        public LazyVariable(string key, Func<object> func)
            : this(key, new Lazy<object>(func))
        {
        }

        public LazyVariable(string key, object value)
            : this(key, () => value)
        {
        }


        public string Key { get; private set; }

        public object Value
        {
            get { return _lazy.Value; }
            set { _lazy = new Lazy<object>(() => value); }
        }
        public Type Type => _lazy.IsValueCreated ? _lazy.Value?.GetType() : null;
        public bool Instantiated => _lazy.IsValueCreated;

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
            var result = args.CreateResult(success);
            Executed = true;
            return result;
        }

        public object Clone()
        {
            var result = new LazyVariable(Key, _lazy);
            return result;
        }
    }
}
