using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Xml.Linq;
using OfficeOpenXml;
using Reportz.Helpers.Excel.Attributes;
using Reportz.Helpers.Excel.Mappings;
using Reportz.Scripting.Interfaces;

namespace Reportz.Helpers.Excel.Instructions
{
    [XlsxScriptElementAlias("datatable-to-worksheet")]
    [XlsxScriptElementAlias("apply-datatable-to-worksheet")]
    public class ApplyDataTableToWorksheetInstruction : XlsxInstructionBase
    {
        private XElement _element;
        private IScriptParser _parser;

        public ApplyDataTableToWorksheetInstruction()
        {
            
        }

        public override void Configure(IScriptParser parser, XElement element)
        {
            _parser = parser;
            _element = element;
        }

        public override object Execute(ExcelPackage package, ExcelWorksheet worksheet, IExecutableArgs args)
        {
            var dataElem = _element.Element("data");
            if (dataElem == null || dataElem.IsEmpty)
                throw new InvalidOperationException();

            // Get data
            object data;
            if (dataElem.HasElements)
            {
                var rootDataElem = dataElem.Elements().First();
                data = _parser.InstantiateElement(rootDataElem);
            }
            else
            {
                data = _parser.EvaluateExpression(args.Scope, dataElem.Value);
            }

            var val = data as IHasValue;
            if (val != null)
                data = val.GetValue();

            // Get data (backwards compatibility)
            var dataTable = data as DataTable;
            if (dataTable == null)
            {
                // todo: keep for backwards compatibility?
                var dt = args?.Arguments?.FirstOrDefault(x => x.Key == "data")?.Value;
                dt = (dt as IExecutableResult)?.Result ?? dt;
                dataTable = dt is DataTable
                    ? (DataTable)dt
                    : null;

                if (dataTable == null)
                    throw new InvalidOperationException("Invalid 'dataTable' value");
            }


            // Apply data
            ExcelWorksheet ws = worksheet;
            if (dataTable != null)
            {
                // Get mappings
                var mappings = new List<Mapping>();
                var mappingsElem = _element.Element("mappings");
                if (mappingsElem != null)
                {
                    foreach (var mapElem in mappingsElem.Elements())
                    {
                        var mappingObj = _parser.InstantiateElement(mapElem);
                        var mapping = mappingObj as Mapping;
                        if (mapping == null)
                            continue;
                        mappings.Add(mapping);
                    }
                }

                var hasMaps = mappings.Any();
                if (hasMaps)
                {
                    foreach (var mapping in mappings)
                    {
                        var colMap = mapping as DataTableColumnMapping;
                        if (colMap != null)
                        {
                            colMap.Apply(dataTable, ws);
                            continue;
                        }
                    }
                }
                else
                {
                    for (var y = 0; y < dataTable.Columns.Count; y++)
                    {
                        for (var x = 0; x < dataTable.Rows.Count; x++)
                        {
                            // apply dataTable to Worksheet

                            var value = dataTable.Rows[x][y];
                            //ws.Cells[y, x].Value = value;
                            ws.SetValue(x + 1, y + 1, value);
                        }
                    }
                }
            }

            object res = null;
            return res;
        }
    }
}
