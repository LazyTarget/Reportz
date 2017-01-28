using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Reportz.Scripting.Attributes;
using Reportz.Scripting.Commands;
using Reportz.Scripting.Interfaces;

namespace Reportz.Scripting.Classes
{
    [ScriptElementAlias("if-group")]
    //[ScriptElement("list")]
    public class IfGroup : IEnumerable<IfCommand>, IExecutable, IScriptElement
    {
#if DEBUG
        public readonly IList<IScriptElement> _children;
#else
        public readonly IList<IScriptElement> _children;
#endif

        private IScriptParser _instantiator;
        private XElement _element;


        public IfGroup()
        {
            _children = new List<IScriptElement>();
        }


        public IEnumerator<IfCommand> GetEnumerator()
        {
            return _children.OfType<IfCommand>().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IExecutableResult Execute(IExecutableArgs args)
        {
            try
            {
                args = args ?? new ExecutableArgs { Scope = new VariableScope() };
                var a = new ExecutableArgs
                {
                    Scope = args.Scope,
                    Arguments = null,
                };

                foreach (var child in _children)
                {
                    var ifGroup = child as IfGroup;
                    if (ifGroup != null)
                    {
                        var res = ifGroup.Execute(a);
                        var success = res?.Result != null;
                        if (success)
                            return res;
                        continue;
                    }

                    var ifCommand = child as IfCommand;
                    if (ifCommand != null)
                    {
                        var matchConditions = ifCommand.VerifyConditions(a);
                        if (matchConditions)
                        {
                            var result = ifCommand.Execute(a);
                            return result;
                        }
                        continue;
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public void Configure(IScriptParser parser, XElement element)
        {
            _instantiator = parser;
            _element = element;


            _children.Clear();

            var children = element.Elements();
            foreach (var child in children)
            {
                var childElem = child;
                var childName = childElem.Name.LocalName.ToLower();

                if (childName == "if" ||
                    childName == "elseif" ||
                    childName == "else")
                {
                    var obj = parser.InstantiateElement(childElem);
                    var elem = (IScriptElement) obj;
                    _children.Add(elem);
                }
                else
                {
                    throw new NotSupportedException();
                }
            }
        }
    }
}
