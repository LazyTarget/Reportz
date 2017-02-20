using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Reportz.Scripting.Interfaces;

namespace Reportz.Scripting.Classes
{
    public class VariableScope
    {
        private readonly IDictionary<string, IVariable> _data;

        public VariableScope()
        {
            _data = new ConcurrentDictionary<string, IVariable>();
        }

        public virtual VariableScope Parent { get; private set; }

        public virtual VariableScope Root
        {
            get
            {
                VariableScope p = this;
                while (p.Parent != null)
                {
                    p = p.Parent;
                }
                return p;
            }
        }


        public virtual VariableScope CreateChild()
        {
            var child = new VariableScope
            {
                Parent = this,
            };
            return child;
        }

        public virtual IVariable GetVariable(string key)
        {
            var localOnly = false;
            //var localOnly = key.StartsWith("$$");
            //if (localOnly)
            //    key = key.Substring("$$".Length);

            IVariable result;
            if (!_data.TryGetValue(key, out result))
            {
                if (Parent != null && !localOnly)
                {
                    result = Parent.GetVariable(key);
                }
            }
            return result;
        }

        public virtual void SetVariable(IVariable variable)
        {
            var v = GetVariable(variable.Key);
            if (v != null)
            {
                //v.Value = variable.Value;

                var copy = (IVariable) v.Clone();
                copy.Value = variable.Value;
                _data[copy.Key] = copy;
            }
            else
            {
                _data[variable.Key] = variable;
            }
        }

    }
}
