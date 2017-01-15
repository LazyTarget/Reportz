using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Reportz.Scripting.Interfaces;
using Reportz.Scripting.Xml;

namespace Reportz.Scripting.Classes
{
    public class ArgCollection : IEnumerable<IVariable>, IXConfigurable
    {
#if DEBUG
        public readonly IDictionary<string, IVariable> _vars;
#else
        private readonly IDictionary<string, IVariable> _vars;
#endif

        private IXInstantiator _instantiator;
        private XElement _element;


        public ArgCollection()
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

        public void Configure(IXInstantiator instantiator, XElement element)
        {
            _instantiator = instantiator;
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
                         childName == "variable")
                {
                    if (childName == "add")
                        childElem.Name = "variable";
                    var obj = instantiator.InstantiateElement(childElem);
                    var e = (IVariable) obj;
                    _vars[e.Key] = e;
                }
                else if (childName == "remove")
                {
                    childElem.Name = "variable";
                    var obj = instantiator.InstantiateElement(childElem);
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
