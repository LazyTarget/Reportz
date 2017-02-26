using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using OfficeOpenXml;
using Reportz.Helpers.Excel.Instructions;
using Reportz.Scripting;
using Reportz.Scripting.Classes;
using Reportz.Scripting.Interfaces;

namespace Reportz.Helpers.Excel
{
    public class XlsxUpdator : IExecutable
    {
        public XlsxUpdator()
        {
            
        }

        public IExecutableResult Execute(IExecutableArgs args)
        {
            ExcelPackage pkg = null;
            try
            {
                var doc = args?.Arguments?.FirstOrDefault(x => x.Key == "document")?.Value;
                if (doc is ExcelPackage)
                    doc = ((ExcelPackage) doc).Workbook;
                if (doc is ExcelWorkbook)
                    doc = ((ExcelWorkbook) doc).Worksheets.FirstOrDefault();
                var ws = doc as ExcelWorksheet;
                if (ws == null)
                {
                    var fileName = args?.Arguments?.FirstOrDefault(x => x.Key == "loadFileName")?.Value?.ToString()
                                   ?? "ExcelDoc.xlsx";
                    var fileInfo = new FileInfo(fileName);
                    pkg = new ExcelPackage(fileInfo);


                    var sheetName = args?.Arguments?.FirstOrDefault(x => x.Key == "sheetName")?.Value?.ToString();

                    if (!string.IsNullOrEmpty(sheetName))
                    {
                        if (pkg.Workbook.Worksheets.All(x => x.Name != sheetName))
                            ws = pkg.Workbook.Worksheets.Add(sheetName);
                        else
                            ws = pkg.Workbook.Worksheets[sheetName];
                    }
                    else
                        ws = pkg.Workbook.Worksheets.FirstOrDefault();
                }


                //  Refactored to ApplyDataTableToWorksheetInstruction.cs
                //var dt = args?.Arguments?.FirstOrDefault(x => x.Key == "data")?.Value;
                //dt = (dt as IExecutableResult)?.Result ?? dt;
                //var dataTable = dt is DataTable
                //    ? (DataTable) dt
                //    : null;
                //if (dataTable != null)
                //{
                //    for (var y = 0; y < dataTable.Columns.Count; y++)
                //    {
                //        for (var x = 0; x < dataTable.Rows.Count; x++)
                //        {
                //            // apply dataTable to Worksheet

                //            var value = dataTable.Rows[x][y];
                //            //ws.Cells[y, x].Value = value;
                //            ws.SetValue(x + 1, y + 1, value);
                //        }
                //    }
                //}
                
                var instructions =
                    args?.Arguments?.FirstOrDefault(x => x.Key == "instructions")?.Value as
                        IEnumerable<IXlsxInstruction>;
                if (instructions != null)
                {
                    foreach (var inst in instructions)
                    {
                        var r = inst.Execute(pkg, ws, args);
                        if (r is ExcelWorksheet)
                            ws = (ExcelWorksheet) r;
                    }
                }


                object res = pkg;

                var result = args.CreateResult(res);
                return result;
            }
            catch (Exception ex)
            {
                throw;
            }
            finally
            {
                var fileName = args?.Arguments?.FirstOrDefault(x => x.Key == "saveFileName")?.Value?.ToString();
                if (!string.IsNullOrWhiteSpace(fileName))
                {
                    var fileInfo = new FileInfo(fileName);
                    pkg?.SaveAs(fileInfo);
                }
                else
                    pkg?.Save();
            }
        }

    }
}
