using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using OfficeOpenXml;
using Reportz.Helpers.Excel.Attributes;
using Reportz.Scripting.Interfaces;

namespace Reportz.Helpers.Excel.Instructions
{
    [XlsxScriptElementAlias("delete-worksheet")]
    public class DeleteWorksheetInstruction : XlsxInstructionBase
    {
        public DeleteWorksheetInstruction()
        {
            
        }

        public string SheetName { get; set; }


        public override void Configure(IScriptParser parser, XElement element)
        {
            SheetName = element?.Attribute("sheetName")?.Value;
        }

        public override object Execute(ExcelPackage package, ExcelWorksheet worksheet, IExecutableArgs args)
        {
            ExcelWorksheet ws = null;
            var sheetName = SheetName;
            if (string.IsNullOrWhiteSpace(sheetName))
            {
                //throw new ArgumentException();
                sheetName = worksheet?.Name;
            }

            if (package.Workbook.Worksheets.Any(x => x.Name == sheetName))
            {
                package.Workbook.Worksheets.Delete(sheetName);
                ws = package.Workbook.Worksheets.FirstOrDefault();
                if (ws == null)
                {

                }
            }
            else
            {
                throw new Exception($"Could not find worksheet to remove. SheetName: {sheetName}. ");
            }
            return ws;
        }
    }
}
