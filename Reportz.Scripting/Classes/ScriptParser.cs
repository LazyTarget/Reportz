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
        private readonly IDictionary<string, Type> _knownAliases;

        private IScriptContext _scriptContext;


        public ScriptParser()
        {
            AppDomain.CurrentDomain.AssemblyLoad += CurrentDomainOnAssemblyLoad;

            _xmlSettings = new Lux.Serialization.Xml.XmlSettings();

            _knownAliases = new SortedDictionary<string, Type>();

            // defaults
            _knownAliases["script"] = typeof (Script);
            _knownAliases["variable"] = typeof(Variable);
            _knownAliases["event"] = typeof(Event);
            _knownAliases["alert"] = typeof(AlertCommand);
            _knownAliases["run-executable"] = typeof(RunExecutableCommand);
            _knownAliases["invoke-method"] = typeof(InvokeMethodCommand);
            _knownAliases["events"] = typeof(EventCollection);
            _knownAliases["arguments"] = typeof(ArgCollection);
            _knownAliases["list"] = typeof(ArgCollection);

            // reflection
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                LoadScriptElementAliases(assembly);
            }

            _scriptContext = new ScriptContext();
        }


        protected virtual IExpressionEvaluator GetExpressionEvaluator(VariableScope scope)
        {
            var evaluator = new ScopedExpressionEvaluator(scope);
            return evaluator;
        }


        public virtual IScriptDocument ParseDocument(string text)
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
                    var scripts = (IEnumerable<IScript>) obj;
                    foreach (var script in scripts)
                    {
                        _scriptContext.ScriptScope.AppendScript(script);
                    }

                    result = new ScriptDocument
                    {
                        Context = _scriptContext,
                    };
                }
                else if (obj is IScript)
                {
                    var script = (IScript) obj;
                    _scriptContext.ScriptScope.AppendScript(script);

                    result = new ScriptDocument
                    {
                        Context = _scriptContext,
                    };
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
                    // todo: move 'instantiate' to Execute()...
                    // todo: ...refactor as a Command?

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
                                child.Name = "variable";        // todo: improve, use ArgCollection...

                                var el = InstantiateElement(child);
                                var variable = el as IVariable;
                                var val = variable?.Value ?? el;
                                arguments.Add(val);
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
                            child.Name = "variable";        // todo: improve, use ArgCollection...

                            var el = InstantiateElement(child);
                            var variable = el as IVariable;
                            var val = variable?.Value ?? el;
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
                }
                else if (TryResolveElementAlias(elementName, out type))
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
                        _AttachInternal(instance);

                        var configurable = (IScriptElement) instance;
                        configurable.Configure(this, element);
                        value = configurable;
                    }
                    else if (!type.IsPrimitive && type != typeof (string) && !type.IsValueType)
                    {
                        var temp = _xmlSettings.TypeInstantiator.Instantiate(type, arguments?.ToArray());
                        _AttachInternal(temp);
                        value = temp;
                    }
                    else
                    {
                        value = _xmlSettings.Converter.Convert(elementValue, type);
                        _AttachInternal(value);
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
                                    _AttachInternal(val);
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

        protected internal virtual void _AttachInternal(object obj)
        {
            if (obj is IHasScriptContext)
            {
                var hasScriptContext = (IHasScriptContext) obj;
                hasScriptContext.Context = _scriptContext;
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


        public virtual bool TryResolveElementAlias(string elementName, out Type type)
        {
            var resolved = _knownAliases.TryGetValue(elementName, out type);
            return resolved;
        }


        private void LoadScriptElementAliases(Assembly assembly)
        {
            var types = assembly
                .GetTypes()
                .Where(x => typeof (IScriptElement).IsAssignableFrom(x) && x.IsClass)
                .ToArray();
            foreach (var type in types)
            {
                var scriptElementAttrs = type.GetCustomAttributes<ScriptElementAliasAttribute>(inherit: false).ToArray();
                if (scriptElementAttrs.Any())
                {
                    foreach (var attr in scriptElementAttrs)
                        _knownAliases[attr.Alias] = type;
                }
            }
        }
        
        private void CurrentDomainOnAssemblyLoad(object sender, AssemblyLoadEventArgs args)
        {
            LoadScriptElementAliases(args.LoadedAssembly);
        }

    }
}
