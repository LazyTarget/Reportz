using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using Reportz.Scripting.Attributes;
using Reportz.Scripting.Commands;
using Reportz.Scripting.Interfaces;

namespace Reportz.Scripting.Classes
{
    public class ScriptParser : IScriptParser
    {
        private readonly Lux.Serialization.Xml.XmlSettings _xmlSettings = new Lux.Serialization.Xml.XmlSettings();
        //private readonly Lux.Interfaces.ITypeInstantiator _typeInstantiator = new Lux.TypeInstantiator();

#if DEBUG
        public readonly IDictionary<string, Type> _knownTypes;
#else
        private readonly IDictionary<string, Type> _knownTypes;
#endif

        public ScriptParser()
        {
            _xmlSettings = new Lux.Serialization.Xml.XmlSettings();

            _knownTypes = new SortedDictionary<string, Type>();

            // defaults
            _knownTypes["script"] = typeof (Script);
            _knownTypes["variable"] = typeof(Variable);
            _knownTypes["event"] = typeof(Event);
            _knownTypes["alert"] = typeof(AlertCommand);
            _knownTypes["run-executable"] = typeof(RunExecutableCommand);
            _knownTypes["invoke-method"] = typeof(InvokeMethodCommand);
            _knownTypes["events"] = typeof(EventCollection);
            _knownTypes["arguments"] = typeof(ArgCollection);
            _knownTypes["list"] = typeof(ArgCollection);

            // reflection
            var types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(x => x.GetTypes())
                .Where(x => typeof (IScriptElement).IsAssignableFrom(x) && x.IsClass)
                .ToArray();
            foreach (var type in types)
            {
                var scriptElementAttrs = type.GetCustomAttributes<ScriptElementAliasAttribute>(inherit: false).ToArray();
                if (scriptElementAttrs.Any())
                {
                    foreach (var attr in scriptElementAttrs)
                        _knownTypes[attr.Alias] = type;
                }
            }
        }


        protected virtual IExpressionEvaluator GetExpressionEvaluator(VariableScope scope)
        {
            var evaluator = new ScopedExpressionEvaluator(scope);
            return evaluator;
        }


        public IScriptDocument ParseDocument(string text)
        {
            try
            {
                var xmlDoc = XDocument.Parse(text);
                var obj = InstantiateElement(xmlDoc.Root);

                IScriptDocument result;
                if (obj is IScriptDocument)
                {
                    result = (IScriptDocument) obj;
                }
                else if (obj is IEnumerable<IScript>)
                {
                    //var xDoc = new XElement("document");
                    //var xScripts = new XElement("scripts");
                    //xScripts.Add(xmlDoc.Root);
                    //xDoc.Add(xScripts);

                    //var doc = new ScriptDocument();
                    //doc.Configure(this, xDoc);
                    //result = doc;

                    var scripts = (IEnumerable<IScript>) obj;
                    result = new ScriptDocument(scripts);
                }
                else if (obj is IScript)
                {
                    var script = (IScript) obj;
                    result = new ScriptDocument(new IScript[] {script});
                }
                else
                {
                    throw new InvalidOperationException();
                }
                return result;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        
        public virtual object InstantiateElement(XElement element)
        {
            try
            {
                var elementName = element.Name.LocalName.ToLower();
                var elementType = element.Attribute("type")?.Value;

                List<object> arguments = null;
                List<Tuple<string, object>> properties = null;
                Type type = null;
                if (elementName == "instantiate")
                {
                    var instantiateElem = element;
                    elementType = instantiateElem?.Element("type")?.Value;
                    if (string.IsNullOrEmpty(elementType))
                        throw new Exception($"<Type> element is required in <Instantiate>.");
                    var typeResolved = TryResolveType(elementType, out type);
                    if (!typeResolved || type == null)
                        throw new Exception($"Type '{elementType}' not found");
                    elementType = type.AssemblyQualifiedName;

                    var ctorElem = instantiateElem.Element("ctor");
                    if (ctorElem != null)
                    {
                        var argsChildren = ctorElem.Element("arguments")?.Elements();
                        if (argsChildren != null)
                        {
                            arguments = new List<object>();
                            foreach (var child in argsChildren)
                            {
                                var arg = InstantiateElement(child);
                                arguments.Add(arg);
                            }
                        }
                    }

                    var propsElem = instantiateElem.Element("properties");
                    if (propsElem != null)
                    {
                        properties = new List<Tuple<string, object>>();
                        var propChildren = propsElem.Elements();
                        foreach (var child in propChildren)
                        {
                            var propVal = InstantiateElement(child);
                            var key = child.Attribute("key")?.Value;
                            var prop = new Tuple<string, object>(key, propVal);
                            properties.Add(prop);
                        }
                    }
                }
                else if (_knownTypes.TryGetValue(elementName, out type))
                {
                    if (type == null)
                        throw new Exception($"Type for element '{elementName}' not found");
                }
                else if (!string.IsNullOrEmpty(elementType))
                {
                    var typeResolved = TryResolveType(elementType, out type);
                    if (!typeResolved || type == null)
                        throw new Exception($"Type '{elementType}' not found");
                    elementType = type.AssemblyQualifiedName;
                }
                else
                {
                    throw new InvalidOperationException($"Could not identify ScriptElement '{elementName}'");
                }


                object value;
                var elementValue = element.Attribute("value")?.Value;
                if (elementValue == null && !string.IsNullOrEmpty(element.Value) && !element.HasElements)
                    elementValue = element.Value;
                if (!string.IsNullOrEmpty(elementValue))
                    value = elementValue;

                if (type == typeof (string))
                {
                    value = elementValue;
                }
                else if (type != null)
                {
                    if (typeof (IScriptElement).IsAssignableFrom(type))
                    {
                        var instance = _xmlSettings.TypeInstantiator.Instantiate(type, arguments?.ToArray());
                        var configurable = (IScriptElement) instance;
                        configurable.Configure(this, element);
                        value = configurable;
                    }
                    else if (!type.IsPrimitive && type != typeof (string) && !type.IsValueType)
                    {
                        var temp = _xmlSettings.TypeInstantiator.Instantiate(type, arguments?.ToArray());
                        value = temp;
                    }
                    else
                    {
                        value = _xmlSettings.Converter.Convert(elementValue, type);
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
                    value = elementValue;
                }
                return value;
            }
            catch (Exception ex)
            {
                //_log.Error($"Error when instantiating obj", ex);
                throw;
            }
        }

        
        public virtual object EvaluateExpression(VariableScope scope, string expression)
        {
            var evaluator = GetExpressionEvaluator(scope);
            var result = evaluator.EvaluateExpression(expression);
            return result;
        }
        

        public virtual bool TryResolveType(string typeName, out Type type)
        {
            type = null;
            if (string.IsNullOrWhiteSpace(typeName))
                return false;
            type = Type.GetType(typeName, false, true);
            if (type == null)
            {
                type = Type.GetType("System." + typeName, false, true);
            }
            if (type == null)
            {
                var mscorlib = typeof (Object).Assembly;
                type = mscorlib.GetType(typeName, false, true);
            }

            var resolved = type != null;
            return resolved;
        }
    }
}
