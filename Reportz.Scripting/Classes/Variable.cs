using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Reportz.Scripting.Interfaces;
using Reportz.Scripting.Xml;

namespace Reportz.Scripting.Classes
{
    //public class Variable : IVariable
    //{
    //    private Type _explicitType;

    //    public string Key { get; set; }
    //    public object Value { get; set; }
    //    public Type Type => _explicitType ?? Value?.GetType();
    //}

    public class Variable : IVariable, IExecutable, IXConfigurable
    {
        private IXInstantiator _instantiator;
        private Type _explicitType;
        private XElement _element;

        public Variable()
        {
            
        }

        public string Key { get; set; }
        public object Value { get; set; }
        public Type Type => _explicitType ?? Value?.GetType();
        public bool Instantiated { get; private set; }
        
        public IExecutableResult Execute(IExecutableArgs args)
        {
            // instantiate
            var instantiateElem = _element?.Element("instantiate");
            if (instantiateElem != null)
            {
                if (_instantiator != null)
                {
                    var value = _instantiator.InstantiateElement(instantiateElem);
                    Value = value;
                    Instantiated = true;
                }
                else
                {
                    throw new InvalidOperationException("Missing instantiator!");
                }
            }
            else if (_element?.Attribute("var") != null)
            {
                var key = _element.Attribute("var")?.Value;
                var val = args.Scope?.GetVariable(key)?.Value;
                Value = val;
            }
            
            if (Value is string)
            {
                // todo: refactor to expression evaluator...

                var words = Value.ToString().Split(' ').ToArray();
                for (var i = 0; i < words.Length; i++)
                {
                    var word = words[i];
                    if (word.ElementAtOrDefault(0) == '$')
                    {
                        if (word.ElementAtOrDefault(1) == '$')
                        {
                                
                        }

                        var key = word.TrimStart('$');
                        var variable = args.Scope?.GetVariable(key);
                        var val = variable?.Value;
                        word = val?.ToString();
                        words[i] = word;
                    }
                }
                Value = string.Join(" ", words);
            }

            args.Scope?.Parent?.SetVariable(this);

            var success = args.Scope?.Parent?.GetVariable(Key) != null &&
                          args.Scope?.Parent?.GetVariable(Key).Value == Value;
            var result = new ExecutableResult
            {
                Result = success,
            };
            return result;
        }

        public void Configure(IXInstantiator instantiator, XElement element)
        {
            _element = element;
            _instantiator = instantiator;

            Instantiated = element.Element("instantiate") == null;
            Key = element.Attribute("key")?.Value;
            Value = element.Attribute("value")?.Value;

            if (element.Attribute("value") == null && !element.HasElements)
            {
                Value = element.Value;
            }

            var typeName = ScriptParser.MatchTypeAlias(element.Attribute("type")?.Value);
            if (!string.IsNullOrWhiteSpace(typeName))
            {
                _explicitType = Type.GetType(typeName, false, true);
                if (_explicitType != null)
                {
                    var typeConverter = new Lux.Serialization.Xml.XmlSettings().Converter;
                    Value = typeConverter.Convert(Value, _explicitType);
                }
            }
        }
    }
}
