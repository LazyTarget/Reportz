using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Reportz.Scripting.Attributes;
using Reportz.Scripting.Classes;
using Reportz.Scripting.Interfaces;

namespace Reportz.Scripting.Commands
{
    [ScriptElementAlias("if")]
    [ScriptElementAlias("elseif")]
    [ScriptElementAlias("else")]
    public class IfCommand : IExecutable, IScriptElement
    {
        private ConditionsCollection _conditionsCollection;
        private IExecutableEnvironment _execute;
        private IScriptParser _scriptParser;
        private XElement _element;


        public bool VerifyConditions(IExecutableArgs args)
        {
            bool result;
            if (_conditionsCollection != null)
            {
                var vars = _conditionsCollection._vars.Values.Select(x =>
                {
                    x.Execute(args);
                    return x.Value;
                }).ToArray();

                if (vars.Any())
                {
                    result = true;
                    foreach (var v in vars)
                    {
                        bool b = v is bool && !(bool)v;
                        b = b || (bool.TryParse(v?.ToString(), out b) && !b);
                        if (b)
                        {
                            result = false;
                            break;
                        }

                        result = result && !string.IsNullOrWhiteSpace(v?.ToString());
                    }
                }
                else
                {
                    result = true;
                }
            }
            else
            {
                var tmp = _element?.Attribute("cond")?.Value ??
                          _element?.Attribute("condition")?.Value;
                if (!string.IsNullOrWhiteSpace(tmp))
                {
                    bool.TryParse(tmp, out result);
                }
                else
                {
                    result = true;
                }
            }
            return result;
        }
        
        public IExecutableResult Execute(IExecutableArgs args)
        {
            if (_execute == null)
                throw new InvalidOperationException("Execute logic hasn't been loaded");
            args = args ?? new ExecutableArgs { Scope = new VariableScope() };
            var result = _execute?.Execute(args);
            return result;
        }

        public void Configure(IScriptParser parser, XElement element)
        {
            _scriptParser = parser;
            _element = element;
            
            var conditionsElem = element.Element("conditions");
            if (conditionsElem != null)
            {
                var conditions = new ConditionsCollection();
                conditions.Configure(parser, conditionsElem);
                _conditionsCollection = conditions;
            }

            var executeElem = element.Element("execute") ?? element;
            if (executeElem != null)
            {
                var exe = new ExecutableEnvironment();
                exe.Configure(parser, executeElem);
                _execute = exe;
            }
        }
    }
}
