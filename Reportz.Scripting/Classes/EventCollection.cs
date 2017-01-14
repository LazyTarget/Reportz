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
    public class EventCollection : IEnumerable<IEvent>, IXConfigurable
    {
#if DEBUG
        public readonly IDictionary<string, IEvent> _events;
#else
        private readonly IDictionary<string, IEvent> _events;
#endif

        public IEnumerator<IEvent> GetEnumerator()
        {
            return _events.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Configure(IXInstantiator instantiator, XElement element)
        {
            _events.Clear();

            var children = element.Elements();
            foreach (var child in children)
            {
                var childName = child.Name.LocalName.ToLower();
                if (childName == "add" ||
                    childName == "item")
                {
                    var e = new Event();
                    e.Configure(instantiator, child);
                    _events[e.Key] = e;
                }
                else if (childName == "remove")
                {
                    var e = new Event();
                    e.Configure(instantiator, child);
                    var r = _events.Remove(e.Key);
                }
                else
                {
                    throw new NotSupportedException();
                }
            }
        }
    }
}
