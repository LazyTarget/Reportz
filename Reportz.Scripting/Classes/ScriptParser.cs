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
    public class ScriptParser : IXInstantiator
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
            _knownTypes["script"] = typeof (Script);
            _knownTypes["variable"] = typeof(Variable);
            _knownTypes["event"] = typeof(Event);
        }


        public IScript Parse(string text)
        {
            try
            {
                var doc = XDocument.Parse(text);
                var obj = InstantiateElement(doc.Root);

                IScript result;
                var scriptCollection = obj as IEnumerable<IScript>;
                if (scriptCollection != null)
                    result = scriptCollection.FirstOrDefault();
                else
                    result = obj as IScript;
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
                if (!string.IsNullOrEmpty(elementType))
                {
                    type = Type.GetType(elementType);
                    if (type == null)
                        throw new Exception($"Type '{elementType}' not found");
                }
                else if (element.Elements().Count() == 1 && element.Element("instantiate") != null)
                {
                    var instantiateElem = element.Element("instantiate");
                    elementType = instantiateElem?.Element("type")?.Value;
                    if (string.IsNullOrEmpty(elementType))
                        throw new Exception($"<Type> element is required in <Instantiate>.");
                    type = Type.GetType(elementType);
                    if (type == null)
                        throw new Exception($"Type '{elementType}' not found");

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
                else if (element.Attribute("type") == null)
                {
                    // The type to instanciate has not been explicitly specified
                    var f = _knownTypes.TryGetValue(elementName, out type);
                    if (type == null)
                        throw new Exception($"Type for element '{elementName}' not found");
                }
                else
                {

                }


                object value;
                var elementValue = element.Attribute("value")?.Value;
                if (elementValue == null && !string.IsNullOrEmpty(element.Value))
                    elementValue = element.Value;
                if (!string.IsNullOrEmpty(elementValue) && !element.HasElements)
                {
                    value = elementValue;
                }

                if (type != null)
                {
                    if (typeof(IXConfigurable).IsAssignableFrom(type))
                    {
                        var instance = _xmlSettings.TypeInstantiator.Instantiate(type, arguments?.ToArray());
                        var configurable = (IXConfigurable) instance;
                        configurable.Configure(this, element);
                        value = configurable;
                    }
                    else if (!type.IsPrimitive && type != typeof(string))
                    {
                        var temp = Activator.CreateInstance(type);
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
                                propInfo.SetValue(value, property.Item2);
                            else
                            {
                                
                            }
                        }
                    }
                }
                else
                    value = elementValue;
                return value;
            }
            catch (Exception ex)
            {
                //_log.Error($"Error when instantiating obj", ex);
                throw;
            }
        }
    }
}
