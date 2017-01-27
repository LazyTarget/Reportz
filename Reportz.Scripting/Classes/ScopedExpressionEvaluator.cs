using System;
using Reportz.Scripting.Interfaces;

namespace Reportz.Scripting.Classes
{
    public class ScopedExpressionEvaluator : ExpressionEvaluator, IExpressionEvaluator
    {
        private readonly VariableScope _scope;

        public ScopedExpressionEvaluator(VariableScope scope)
        {
            _scope = scope;
        }
        

        protected virtual bool TryLookupVariable(string key, out IVariable variable)
        {
            variable = _scope?.GetVariable(key);
            while (variable == null && key.StartsWith("$"))
            {
                key = key.Substring(1);
                variable = _scope?.GetVariable(key);
            }
            return variable != null;
        }

        protected override bool TryLookupKey(string key, out Func<string, object> invoker)
        {
            // todo: lookup in base first?

            IVariable variable;
            if (TryLookupVariable(key, out variable))
            {
                invoker = (k) => variable.Value;
                return variable != null;
            }
            else
            {
                return base.TryLookupKey(key, out invoker);
            }
        }


    }
}
