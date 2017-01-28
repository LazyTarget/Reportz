using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Reportz.Scripting.Attributes;
using Reportz.Scripting.Interfaces;

namespace Reportz.Scripting.Classes
{
    [ScriptElementAlias("events")]
    public class EventCollection : IEnumerable<IEvent>, IScriptElement
    {
#if DEBUG
        public readonly IDictionary<string, IEvent> _events;
#else
        private readonly IDictionary<string, IEvent> _events;
#endif

        public EventCollection()
        {
            _events = new Dictionary<string, IEvent>();
        }

        public IEnumerator<IEvent> GetEnumerator()
        {
            return _events.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Configure(IScriptParser parser, XElement element)
        {
            _events.Clear();

            var children = element.Elements();
            foreach (var child in children)
            {
                var childName = child.Name.LocalName.ToLower();
                if (childName == "event" ||
                    childName == "add" ||
                    childName == "item")
                {
                    var e = new Event();
                    e.Configure(parser, child);
                    _events[e.Key] = e;
                }
                else if (childName == "remove")
                {
                    var e = new Event();
                    e.Configure(parser, child);
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
