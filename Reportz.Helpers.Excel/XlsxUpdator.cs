using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using OfficeOpenXml;
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
                    var fileInfo = new FileInfo("ExcelDoc.xlsx");
                    pkg = new ExcelPackage(fileInfo);
                    ws = pkg.Workbook.Worksheets.Add("Sheet1");
                    //ws = new ExcelWorksheet();
                }


                var dt = args?.Arguments?.FirstOrDefault(x => x.Key == "data")?.Value;
                dt = (dt as IExecutableResult)?.Result ?? dt;
                var dataTable = dt is DataTable
                    ? (DataTable) dt
                    : null;
                if (dataTable != null)
                {
                    for (var y = 0; y < dataTable.Columns.Count; y++)
                    {
                        for (var x = 0; x < dataTable.Rows.Count; x++)
                        {
                            var value = dataTable.Rows[x][y];
                            //ws.Cells[y, x].Value = value;
                            ws.SetValue(x + 1, y + 1, value);
                        }
                    }
                }

                // todo: apply dataSet to Worksheet
                object res = null;

                var result = new ExecutableResult
                {
                    Result = res,
                };
                return result;
            }
            catch (Exception ex)
            {
                throw;
            }
            finally
            {
                pkg?.Save();
            }
        }
    }
}
