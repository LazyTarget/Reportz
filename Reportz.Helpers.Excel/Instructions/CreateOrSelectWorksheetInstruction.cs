using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using OfficeOpenXml;
using Reportz.Helpers.Excel.Attributes;
using Reportz.Scripting.Interfaces;

namespace Reportz.Helpers.Excel.Instructions
{
    [XlsxScriptElementAlias("select-worksheet")]
    [XlsxScriptElementAlias("create-worksheet")]
    [XlsxScriptElementAlias("create-or-select-worksheet")]
    public class CreateOrSelectWorksheetInstruction : XlsxInstructionBase
    {
        public CreateOrSelectWorksheetInstruction()
        {
            
        }

        public string SheetName { get; set; }


        public override void Configure(IScriptParser parser, XElement element)
        {
            SheetName = element?.Attribute("sheetName")?.Value;
        }

        public override object Execute(ExcelPackage package, ExcelWorksheet worksheet, IExecutableArgs args)
        {
            ExcelWorksheet ws;
            var sheetName = SheetName;
            if (string.IsNullOrWhiteSpace(sheetName))
                throw new ArgumentException();

            if (package.Workbook.Worksheets.All(x => x.Name != sheetName))
                ws = package.Workbook.Worksheets.Add(sheetName);
            else
                ws = package.Workbook.Worksheets[sheetName];
            return ws;
        }
    }
}
