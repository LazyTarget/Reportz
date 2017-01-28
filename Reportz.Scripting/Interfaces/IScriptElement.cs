using System.Xml.Linq;

namespace Reportz.Scripting.Interfaces
{
    public interface IScriptElement
    {
        void Configure(IScriptParser parser, XElement element);
    }
}
