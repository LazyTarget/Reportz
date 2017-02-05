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
            
        }

        public override object Execute(ExcelPackage package, ExcelWorksheet worksheet, IExecutableArgs args)
        {
            ExcelWorksheet ws;
            var sheetName = args?.Arguments?.FirstOrDefault(x => x.Key == "sheetName")?.Value?.ToString()
                            ?? SheetName
                            ?? "Sheet1";
            if (package.Workbook.Worksheets.All(x => x.Name != sheetName))
                ws = package.Workbook.Worksheets.Add(sheetName);
            else
                ws = package.Workbook.Worksheets[sheetName];
            return ws;
        }
    }
}
