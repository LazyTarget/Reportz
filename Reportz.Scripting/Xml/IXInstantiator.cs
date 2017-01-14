using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Reportz.Scripting.Xml
{
    public interface IXInstantiator
    {
        object InstantiateElement(XElement element);
    }
}
