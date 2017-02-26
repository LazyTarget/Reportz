using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Xml.Linq;
using OfficeOpenXml;
using Reportz.Helpers.Excel.Attributes;
using Reportz.Scripting.Classes;
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

        public void Apply(DataTable data, ExcelWorksheet worksheet, VariableScope scope)
        {
            var columns = new List<string>();
            var valueElem = _element.Element("value");
            if (valueElem == null)
            {
                var sourcesElem = _element.Element("sources") ?? _element;
                foreach (var sourceElem in sourcesElem.Elements("source"))
                {
                    columns.Add(sourceElem.Value);
                }

                if (!columns.Any())
                {
                    // Add all columns from DataTable...
                    columns.AddRange(data.Columns.Cast<DataColumn>().Select(c => c.ColumnName));
                }
            }
            
            var targetElem = _element.Element("target");
            if (targetElem != null)
            {
                bool includeHeader;
                bool.TryParse(targetElem.Attribute("includeHeader")?.Value, out includeHeader);

                var target = targetElem.Value;
                var range = worksheet.Cells[target];
                var colCount = range.Columns > 1 ? range.Columns : columns.Count;
                colCount = Math.Max(colCount, 1);
                
                for (var y = 0; y < colCount; y++)
                {
                    var headerIncluded = false;
                    var columnName = columns.ElementAtOrDefault(y);
                    var column = columnName != null ? data.Columns[columnName] : null;
                    var rowCount = range.Rows > 1 ? range.Rows : data.Rows.Count;
                    if (includeHeader)
                        rowCount++;

                    for (var x = 0; x < rowCount; x++)
                    {
                        var cell = range.Offset(x, y, 1, 1);
                        var rowIndex = x;
                        if (headerIncluded)
                            rowIndex--;

                        // Determine type...
                        var type = column?.DataType;
                        var typeName = _element.Attribute("type")?.Value;
                        if (!string.IsNullOrEmpty(typeName))
                        {
                            if (!_parser.TryResolveType(typeName, out type))
                                type = null;
                        }


                        // Determine new value...
                        object value;
                        if (valueElem != null)
                        {
                            var expr = valueElem.Value;
                            value = _parser.EvaluateExpression(scope, expr);
                        }
                        else if (column == null)
                        {
                            // No data to insert
                            value = null;
                        }
                        else if (data.Rows.Count <= rowIndex)
                        {
                            // Assign null (empty)
                            value = null;
                        }
                        else if (includeHeader && !headerIncluded)
                        {
                            // Assign column name
                            value = column.ColumnName;
                            headerIncluded = true;
                        }
                        else
                        {
                            var row = data.Rows[rowIndex];
                            value = row[column.Ordinal];
                        }

                        
                        // Try convert type
                        if (type != null)
                        {
                            var val = value;
                            if (_parser.TryConvertType(type, val, out value))
                                value = val;
                        }
                        
                        // Compare with current value...
                        if (cell.Value != null)
                        {
                            // Cell has a previous value
                            if (!Object.Equals(cell.Value, value))
                            {
                                // Value will be updated...
                            }
                        }
                        else
                        {
                            // Cell has NO previous value
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
