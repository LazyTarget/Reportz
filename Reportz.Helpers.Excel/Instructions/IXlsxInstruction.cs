using OfficeOpenXml;
using Reportz.Scripting.Interfaces;

namespace Reportz.Helpers.Excel.Instructions
{
    public interface IXlsxInstruction : IScriptElement
    {
        object Execute(ExcelPackage package, ExcelWorksheet worksheet, IExecutableArgs args);
    }
}
