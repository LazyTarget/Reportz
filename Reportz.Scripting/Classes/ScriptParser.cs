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
                Type type = null;
                if (!string.IsNullOrEmpty(elementType))
                {
                    type = Type.GetType(elementType);
                    if (type == null)
                        throw new Exception($"Type '{elementType}' not found");
                }
                else if (element.Attribute("type") != null)
                {
                    // The type to instanciate has not been explicitly specified
                    var f = _knownTypes.TryGetValue(elementName, out type);
                    if (type == null)
                        throw new Exception($"Type for element '{elementName}' not found");
                }
                else if (elementName == "instantiate")
                {
                    arguments = new List<object>();
                    var children = element.Elements();
                    foreach (var child in children)
                    {
                        var arg = InstantiateElement(child);
                        arguments.Add(arg);
                    }
                }
                else
                {

                }


                object value;
                var elementValue = element.Attribute("value")?.Value;
                if (elementValue == null && !string.IsNullOrEmpty(element.Value))
                    elementValue = element.Value;
                if (!string.IsNullOrEmpty(elementValue))
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
