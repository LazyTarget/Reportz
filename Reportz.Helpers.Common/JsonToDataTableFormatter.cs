﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Newtonsoft.Json.Linq;
using Reportz.Scripting;
using Reportz.Scripting.Interfaces;

namespace Reportz.Helpers.Common
{
    public class JsonToDataTableFormatter : IExecutable
    {
        public JsonToDataTableFormatter()
        {
            
        }

        public IExecutableResult Execute(IExecutableArgs args)
        {
            try
            {
                var json = args?.Arguments?.FirstOrDefault(x => x.Key == "json")?.Value;
                var jsonTkn = json is JToken
                    ? (JToken) json
                    : JToken.Parse(json?.ToString());
                var includeHeaderTmp = args?.Arguments?.FirstOrDefault(x => x.Key == "includeHeader")?.Value?.ToString();
                bool includeHeader;
                bool.TryParse(includeHeaderTmp, out includeHeader);
                
                var dataTable = new DataTable();
                ParseToken(jsonTkn, dataTable, null, null);

                if (includeHeader)
                {
                    var headerRow = dataTable.NewRow();
                    dataTable.Rows.InsertAt(headerRow, 0);
                    foreach (DataColumn column in dataTable.Columns)
                    {
                        headerRow[column] = column.ColumnName;
                    }
                }

                var result = args.CreateResult(dataTable);
                return result;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private void ParseToken(JToken token, DataTable dataTable, DataRow currentRow, string[] currentPrefixes)
        {
            var objects = new List<JObject>();
            if (token.Type == JTokenType.Array)
            {
                foreach (var tkn in token)
                {
                    var obj = tkn as JObject;
                    if (obj != null)
                        objects.Add(obj);
                }
            }
            else if (token.Type == JTokenType.Object)
            {
                objects.Add((JObject) token);
            }

            
            foreach (var obj in objects)
            {
                var row = currentRow;
                if (row == null)
                {
                    row = dataTable.NewRow();
                    dataTable.Rows.Add(row);
                }

                var props = obj.Properties();
                foreach (var prop in props)
                {
                    var prefixes = currentPrefixes ?? new string[0];
                    prefixes = prefixes.Concat(new [] {prop.Name}).ToArray();

                    var columnName = string.Join(".", prefixes);
                    var column = dataTable.Columns[columnName] ?? dataTable.Columns.Add(columnName);
                    
                    if (prop.Type == JTokenType.Null)
                    {
                        row[column] = null;
                    }
                    else if (prop.Value is JValue)
                    {
                        row[column] = prop.Value.ToString();
                    }
                    else
                    {
                        ParseToken(prop.Value, dataTable, row, prefixes);
                    }
                }
            }
        }
    }
}
