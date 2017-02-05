using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using Reportz.Helpers.Excel.Attributes;
using Reportz.Scripting.Classes;
using Reportz.Scripting.Interfaces;

namespace Reportz.Helpers.Excel
{
    public class XlsxScriptParser : IScriptParser
    {
        private readonly IDictionary<string, Type> _knownAliases; 
        private readonly IScriptParser _parser;

        public XlsxScriptParser(IScriptParser parser)
        {
            _parser = parser;

            _knownAliases = new SortedDictionary<string, Type>();
            var types = GetType().Assembly
                .GetTypes()
                .Where(x => typeof(IScriptElement).IsAssignableFrom(x) && x.IsClass)
                .ToArray();
            foreach (var type in types)
            {
                var scriptElementAttrs = type.GetCustomAttributes<XlsxScriptElementAliasAttribute>(inherit: false).ToArray();
                if (scriptElementAttrs.Any())
                {
                    foreach (var attr in scriptElementAttrs)
                        _knownAliases[attr.Alias] = type;
                }
            }
        }


        public IScriptDocument ParseDocument(string text)
        {
            var res = _parser.ParseDocument(text);
            return res;
        }

        public object InstantiateElement(XElement element)
        {
            var res = _parser.InstantiateElement(element);
            return res;
        }

        public object EvaluateExpression(VariableScope scope, string expression)
        {
            var res = _parser.EvaluateExpression(scope, expression);
            return res;
        }

        public bool TryResolveType(string typeName, out Type type)
        {
            var res = _parser.TryResolveType(typeName, out type);
            return res;
        }

        public bool TryResolveElementAlias(string elementName, out Type type)
        {
            var resolved = _knownAliases.TryGetValue(elementName, out type);
            if (resolved)
                return resolved;
            resolved = _parser.TryResolveElementAlias(elementName, out type);
            return resolved;
        }
    }
}
