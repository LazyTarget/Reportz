using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Reportz.Scripting.Interfaces;

namespace Reportz.Scripting.Classes
{
    public class ReadonlyVariableScope : VariableScope
    {
        private readonly VariableScope _scope;

        public ReadonlyVariableScope(VariableScope scope)
        {
            if (scope == null)
                throw new ArgumentNullException(nameof(scope));
            _scope = scope;
        }

        public override VariableScope Parent => new ReadonlyVariableScope(_scope.Parent);

        public override VariableScope Root => new ReadonlyVariableScope(_scope.Root);


        public override VariableScope CreateChild()
        {
            var child = _scope.CreateChild();
            child = new ReadonlyVariableScope(child);
            return child;
        }

        public override IVariable GetVariable(string key)
        {
            var result = _scope.GetVariable(key);
            return result;
        }

        public override void SetVariable(IVariable variable)
        {
            // Do nothing...
        }
    }
}
