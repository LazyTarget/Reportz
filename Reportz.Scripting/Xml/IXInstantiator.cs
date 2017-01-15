using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Reportz.Scripting.Classes;

namespace Reportz.Scripting.Xml
{
    public interface IXInstantiator
    {
        object InstantiateElement(XElement element);

        object EvaluateExpression(VariableScope scope, string expression);
    }
}
