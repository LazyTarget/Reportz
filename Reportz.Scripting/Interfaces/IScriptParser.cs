using System;
using System.Xml.Linq;
using Reportz.Scripting.Classes;

namespace Reportz.Scripting.Interfaces
{
    public interface IScriptParser
    {
        IScriptDocument ParseDocument(string text);

        object InstantiateElement(XElement element);

        object EvaluateExpression(VariableScope scope, string expression);

        bool TryResolveType(string typeName, out Type type);

        bool TryResolveElementAlias(string elementName, out Type type);
    }
}
