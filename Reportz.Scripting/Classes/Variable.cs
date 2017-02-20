using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Reportz.Scripting.Attributes;
using Reportz.Scripting.Commands;
using Reportz.Scripting.Interfaces;

namespace Reportz.Scripting.Classes
{
    [ScriptElementAlias("var")]
    [ScriptElementAlias("variable")]
    public class Variable : IVariable, IExecutable, IScriptElement, ICloneable
    {
        private InstantiateCommand _instantiate;
        private IScriptParser _parser;
        private Type _explicitType;
        private bool? _instantiated;
        private XElement _element;

        public Variable()
        {
            
        }

        public string Key { get; set; }
        public object Value { get; set; }
        public Type Type => _explicitType ?? Value?.GetType();
        public bool Instantiated => _instantiated ?? _element?.Element("instantiate") == null;

#if DEBUG
        public bool Executed { get; private set; }
#else
        private bool Executed { get; private set; }
#endif


        public IExecutableResult Execute(IExecutableArgs args)
        {
            if (Executed)
            {
                throw new InvalidOperationException("Variable has already been executed");
                return null;
            }
            

            var instantiateElem = _element?.Element("instantiate");
            if (instantiateElem != null)
            {
                // Instantiate value
                if (_instantiate != null)
                {
                    //var value = _parser.InstantiateElement(instantiateElem);
                    //Value = value;
                    //_instantiated = true;

                    var a = new ExecutableArgs
                    {
                        Scope = args.Scope.CreateChild(),
                        Arguments = new IVariable[] { new SimpleVariable("key", Key) },      // todo: remove?
                    };
                    var exeRes = _instantiate.Execute(a);
                    var value = exeRes.Result;
                    Value = value;
                    _instantiated = true;
                }
                else
                {
                    throw new InvalidOperationException("The 'InstantiateCommand' has not been configured!");
                }
            }
            else if (_element?.Attribute("var") != null)
            {
                // Parse value from variable
                var key = _element.Attribute("var")?.Value;
                var val = args.Scope?.GetVariable(key)?.Value;
                if (_explicitType != null)      // todo: should keep?
                {
                    var typeConverter = new Lux.Serialization.Xml.XmlSettings().Converter;
                    val = typeConverter.Convert(val, _explicitType);
                }
                Value = val;
            }


            // Should evaluate value?
            bool? eval = null;
            var evalTmp = _element?.Attribute("eval")?.Value;
            if (!string.IsNullOrWhiteSpace(evalTmp))
                eval = bool.Parse(evalTmp);

            if (Value is string)
            {
                if (eval.HasValue && !eval.Value)
                {
                    // Explicitly disabled eval
                }
                else
                {
                    if (!eval.HasValue)
                    {
                        // Dynamically determine whether to eval
                        var str = Value.ToString();
                        eval = str.StartsWith("$");     // todo: more rules for when to enable evaluation (by default)?
                    }

                    if (eval.GetValueOrDefault())
                    {
                        var val = _parser.EvaluateExpression(args.Scope, Value.ToString());
                        Value = val;
                    }
                    else
                    {

                    }
                }
            }
            else
            {
                
            }


            int depth;
            var rootRaw = args.Arguments?.FirstOrDefault(x => x.Key == "depth")?.Value?.ToString();
            rootRaw     = rootRaw ?? _element?.Attribute("depth")?.Value;
            int.TryParse(rootRaw, out depth);

            if (rootRaw == "root")
            {
                args.Scope?.Root?.SetVariable(this);
            }
            else
            {
                // implement numeric navigation??
                args.Scope?.Parent?.SetVariable(this);
            }

            var success = args.Scope?.Parent?.GetVariable(Key) != null &&
                          args.Scope?.Parent?.GetVariable(Key).Value == Value;
            var result = args.CreateResult(success);
            Executed = true;
            return result;
        }

        public void Configure(IScriptParser parser, XElement element)
        {
            _element = element;
            _parser = parser;
            
            Key = element.Attribute("key")?.Value;
            Value = element.Attribute("value")?.Value;

            if (element.Attribute("value") == null && !element.HasElements)
            {
                Value = element.Value;
            }
            else
            {
                var instantiateElem = _element?.Element("instantiate");
                if (instantiateElem != null)
                {
                    // Instantiation is done in Execute(..)
                    
                    _instantiate = new InstantiateCommand();
                    _instantiate.Configure(parser, instantiateElem);
                }
                else if (element.Attribute("value") == null && element.HasElements)
                {
                    var rootChild = element.Elements().FirstOrDefault();
                    Value = parser.InstantiateElement(rootChild);
                }
            }
            

            var typeName = element.Attribute("type")?.Value;
            if (!string.IsNullOrWhiteSpace(typeName))
            {
                var typeResolved = _parser.TryResolveType(typeName, out _explicitType);
                if (typeResolved && _explicitType != null)
                {
                    var typeConverter = new Lux.Serialization.Xml.XmlSettings().Converter;
                    Value = typeConverter.Convert(Value, _explicitType);
                }
            }
        }

        public object Clone()
        {
            var result = new Variable();
            result.Key = Key;
            result.Value = Value;

            result._explicitType = _explicitType;
            result._element = _element;
            result._parser = _parser;
            result._instantiated = _instantiated;
            return result;
        }
    }
}
