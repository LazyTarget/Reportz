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
    public class XlsxScriptParser : ScriptParser
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


        public override IScriptDocument ParseDocument(string text)
        {
            //var res = _parser.ParseDocument(text);
            var res = base.ParseDocument(text);
            return res;
        }

        public override object InstantiateElement(XElement element)
        {
            //var res = _parser.InstantiateElement(element);
            var res = base.InstantiateElement(element);
            return res;
        }

        public override object EvaluateExpression(VariableScope scope, string expression)
        {
            //var res = _parser.EvaluateExpression(scope, expression);
            var res = base.EvaluateExpression(scope, expression);
            return res;
        }

        public override bool TryResolveType(string typeName, out Type type)
        {
            //var res = _parser.TryResolveType(typeName, out type);
            var res = base.TryResolveType(typeName, out type);
            return res;
        }

        public override bool TryResolveElementAlias(string elementName, out Type type)
        {
            var resolved = _knownAliases.TryGetValue(elementName, out type);
            if (resolved)
                return resolved;
            resolved = _parser.TryResolveElementAlias(elementName, out type);
            return resolved;
        }
    }
}
