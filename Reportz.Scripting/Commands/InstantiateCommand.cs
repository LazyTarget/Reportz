using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using Reportz.Scripting.Attributes;
using Reportz.Scripting.Classes;
using Reportz.Scripting.Interfaces;

namespace Reportz.Scripting.Commands
{
    [ScriptElementAlias("instantiate")]
    public class InstantiateCommand : IExecutable, IScriptElement
    {
        private readonly Lux.Serialization.Xml.XmlSettings _xmlSettings;
        private IScriptParser _parser;
        private XElement _element;

        public InstantiateCommand()
        {
            _xmlSettings = new Lux.Serialization.Xml.XmlSettings();
        }


        public IExecutableResult Execute(IExecutableArgs args)
        {
            IExecutableResult result = null;
            args = args ?? new ExecutableArgs { Scope = new VariableScope() };

            List<object> arguments = null;
            List<Tuple<string, object>> properties = null;
            Type type;
            string elementType;
            object value;
            string variableKey;
            try
            {
                // todo: move 'instantiate' to Execute()...
                // todo: ...refactor as a Command?

                var instantiateElem = _element;
                elementType = instantiateElem?.Element("type")?.Value;
                if (string.IsNullOrEmpty(elementType))
                    throw new Exception($"<Type> element is required in <Instantiate>.");
                var typeResolved = _parser.TryResolveType(elementType, out type);
                if (!typeResolved || type == null)
                    throw new Exception($"Type '{elementType}' not found");
                elementType = type.AssemblyQualifiedName;
                
                var ctorElem = instantiateElem.Element("ctor");
                if (ctorElem != null)
                {
                    var argsChildren = ctorElem.Element("arguments")?.Elements();
                    if (argsChildren != null)
                    {
                        var a = new ExecutableArgs
                        {
                            Scope = args.Scope.CreateChild(),
                            Arguments = null,
                        };

                        arguments = new List<object>();
                        foreach (var child in argsChildren)
                        {
                            child.Name = "variable";        // todo: improve, use ArgCollection...

                            var el = _parser.InstantiateElement(child);
                            var var = el as IVariable;
                            var exeRes = var.Execute(a);
                            var val = var?.Value ?? el;
                            arguments.Add(val);
                        }
                    }
                }

                var propsElem = instantiateElem.Element("properties");
                if (propsElem != null)
                {
                    var a = new ExecutableArgs
                    {
                        Scope = args.Scope.CreateChild(),
                        Arguments = null,
                    };

                    properties = new List<Tuple<string, object>>();
                    var propChildren = propsElem.Elements();
                    foreach (var child in propChildren)
                    {
                        child.Name = "variable";        // todo: improve, use ArgCollection...

                        var el = _parser.InstantiateElement(child);
                        var var = el as IVariable;
                        var exeRes = var.Execute(a);
                        var val = var?.Value ?? el;
                        var key = child.Attribute("key")?.Value;
                        var prop = new Tuple<string, object>(key, val);
                        properties.Add(prop);
                    }

                    //var argColl = new ArgCollection();
                    //argColl.Configure(this, propsElem);
                    //
                    //foreach (var v in argColl._vars)
                    //{
                    //    var a = new ExecutableArgs
                    //    {
                    //        Scope = 
                    //        Arguments = null,
                    //    };
                    //
                    //    var prop = new Tuple<string, object>(v.Key, v.Value?.Value);
                    //    properties.Add(prop);
                    //}
                }


                
                var elementValue = _element.Attribute("value")?.Value;
                if (elementValue == null && !string.IsNullOrEmpty(_element.Value) && !_element.HasElements)
                    elementValue = _element.Value;
                if (!string.IsNullOrEmpty(elementValue))
                    value = elementValue;


                if (type == typeof(string))
                {
                    value = elementValue;
                }
                else if (type != null)
                {
                    if (typeof (IScriptElement).IsAssignableFrom(type))
                    {
                        var instance = _xmlSettings.TypeInstantiator.Instantiate(type, arguments?.ToArray());
                        //_AttachInternal(instance);

                        var configurable = (IScriptElement) instance;
                        configurable.Configure(_parser, instantiateElem);
                        value = configurable;
                    }
                    else if (!type.IsPrimitive && type != typeof (string) && !type.IsValueType)
                    {
                        var temp = _xmlSettings.TypeInstantiator.Instantiate(type, arguments?.ToArray());
                        //_AttachInternal(temp);
                        value = temp;
                    }
                    else
                    {
                        value = _xmlSettings.Converter.Convert(elementValue, type);
                        //_AttachInternal(value);
                    }


                    if (properties != null)
                    {
                        foreach (var property in properties)
                        {
                            var propInfo = type.GetProperty(property.Item1);
                            if (propInfo != null)
                            {
                                var val = property.Item2;
                                try
                                {
                                    val = _xmlSettings.Converter.Convert(val, propInfo.PropertyType);
                                    //_AttachInternal(val);
                                    propInfo.SetValue(value, val);
                                }
                                catch (Exception ex)
                                {
                                    throw;
                                }
                            }
                            else
                            {

                            }
                        }
                    }
                }
                else
                {
                    throw new InvalidOperationException();
                }
                
                
                result = args.CreateResult(value);
                return result;
            }
            catch (Exception ex)
            {
                throw;
                return result;
            }
            finally
            {

            }
        }

        public void Configure(IScriptParser parser, XElement element)
        {
            _parser = parser;
            _element = element;
        }
    }
}
