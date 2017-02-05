using System.Xml.Linq;
using OfficeOpenXml;
using Reportz.Scripting.Interfaces;

namespace Reportz.Helpers.Excel.Instructions
{
    public abstract class XlsxInstructionBase : IXlsxInstruction
    {
        public virtual void Configure(IScriptParser parser, XElement element)
        {
            // todo: populate props with <property>
        }

        public abstract object Execute(ExcelPackage package, ExcelWorksheet worksheet, IExecutableArgs args);
    }
}
