using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Xml.Linq;
using OfficeOpenXml;
using Reportz.Helpers.Excel.Attributes;
using Reportz.Scripting.Interfaces;

namespace Reportz.Helpers.Excel.Mappings
{
    [XlsxScriptElementAlias("col-map")]
    [XlsxScriptElementAlias("column-map")]
    [XlsxScriptElementAlias("column-mapping")]
    public class DataTableColumnMapping : Mapping, IScriptElement
    {
        private IScriptParser _parser;
        private XElement _element;
        
        public void Configure(IScriptParser parser, XElement element)
        {
            _parser = parser;
            _element = element;
        }

        public void Apply(DataTable data, ExcelWorksheet worksheet)
        {
            var sources = new List<string>();
            var sourcesElem = _element.Element("sources") ?? _element;
            foreach (var sourceElem in sourcesElem.Elements("source"))
            {
                sources.Add(sourceElem.Value);
            }

            if (!sources.Any())
            {
                // Add all columns from DataTable...
                sources.AddRange(data.Columns.Cast<DataColumn>().Select(c => c.ColumnName));
            }

            
            var targetElem = _element.Element("target");
            if (targetElem != null)
            {
                var target = targetElem.Value;
                var range = worksheet.Cells[target];
                var colCount = range.Columns > 1 ? range.Columns : sources.Count;
                var rowCount = range.Rows > 1 ? range.Rows : data.Rows.Count;
                
                for (var y = 0; y < colCount; y++)
                {
                    var source = sources.ElementAtOrDefault(y);
                    var column = source != null ? data.Columns[source] : null;

                    for (var x = 0; x < rowCount; x++)
                    {
                        object value;
                        if (column == null)
                        {
                            value = null;
                        }
                        else if (data.Rows.Count < x + 1)
                        {
                            // No data to insert...
                            //continue;

                            //insert blank?
                            value = null;
                        }
                        else
                        {
                            var row = data.Rows[x];
                            value = row[column.Ordinal];
                        }


                        var cell = range.Offset(x, y, 1, 1);
                        if (cell.Value != null)
                        {
                            // Cell has a previous value...
                            if (!Object.Equals(cell.Value, value))
                            {
                                // Value will be updated...
                            }
                        }
                        else
                        {
                            // Cell has NO previous value...
                            if (value != null)
                            {
                                // Value will be inserted...
                            }
                        }
                        cell.Value = value;
                    }
                }
            }
            else
            {
                throw new InvalidOperationException();
            }
        }
    }
}
