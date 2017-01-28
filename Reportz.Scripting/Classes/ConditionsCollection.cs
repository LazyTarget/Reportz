using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Reportz.Scripting.Attributes;
using Reportz.Scripting.Interfaces;

namespace Reportz.Scripting.Classes
{
    [ScriptElementAlias("conditions")]
    public class ConditionsCollection : IEnumerable<IVariable>, IScriptElement
    {
#if DEBUG
        public readonly IDictionary<string, IVariable> _vars;
#else
        private readonly IDictionary<string, IVariable> _vars;
#endif

        private IScriptParser _instantiator;
        private XElement _element;


        public ConditionsCollection()
        {
            _vars = new Dictionary<string, IVariable>();
        }


        public IEnumerator<IVariable> GetEnumerator()
        {
            return _vars.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Configure(IScriptParser parser, XElement element)
        {
            _instantiator = parser;
            _element = element;


            _vars.Clear();

            var children = element.Elements();
            foreach (var child in children)
            {
                var childElem = child;
                var childName = childElem.Name.LocalName.ToLower();
                if (childName == "clear")
                {
                    _vars.Clear();
                }
                else if (childName == "add" ||
                         childName == "item" ||
                         childName == "var" ||
                         childName == "variable" ||
                         childName == "list" ||
                         childName == "condition")
                {
                    if (childName == "add" ||
                        childName == "condition")
                    {
                        childElem.Name = "variable";
                    }

                    var key = childElem.Attribute("key")?.Value;
                    var obj = parser.InstantiateElement(childElem);

                    var variable = obj as IVariable;
                    key = variable?.Key ?? key ?? _vars.Count.ToString();

                    if (variable == null)
                    {
                        variable = new Variable
                        {
                            Key = key,
                            Value = obj,
                        };
                    }
                    else
                    {
                        // todo: must update variable.Key?
                        ((Variable)variable).Key = key;
                    }
                    _vars[key] = variable;
                }
                else if (childName == "remove")
                {
                    childElem.Name = "variable";
                    var obj = parser.InstantiateElement(childElem);
                    var e = (IVariable)obj;
                    var r = _vars.Remove(e.Key);
                }
                else
                {
                    throw new NotSupportedException();
                }
            }
        }
    }
}
