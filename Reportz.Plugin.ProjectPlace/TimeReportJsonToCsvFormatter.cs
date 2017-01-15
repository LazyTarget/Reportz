using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Reportz.Scripting.Classes;
using Reportz.Scripting.Interfaces;

namespace Reportz.Plugin.ProjectPlace
{
    public class TimeReportJsonToCsvFormatter : IExecutable
    {
        public TimeReportJsonToCsvFormatter()
        {
            
        }

        public IExecutableResult Execute(IExecutableArgs args)
        {
            try
            {
                var json = args?.Arguments?.FirstOrDefault(x => x.Key == "json")?.Value;
                var jsonArray = json is JArray
                    ? (JArray) json
                    : JArray.Parse(json?.ToString());
                
                var seperator = args?.Arguments?.FirstOrDefault(x => x.Key == "seperator")?.Value;
                seperator = seperator ?? ";";

                var headerVar = args?.Arguments?.FirstOrDefault(x => x.Key == "header");
                var headers = headerVar?.Value as IEnumerable<IVariable>;
                var headerArr = headers?
                    .Select(x => x.Value?.ToString())
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .ToArray();

                var sb = new StringBuilder();
                if (headerArr != null)
                {
                    foreach (var head in headerArr)
                    {
                        sb.Append(head + seperator);
                    }
                    sb.AppendLine();


                    foreach (var token in jsonArray)
                    {
                        var obj = token as JObject;
                        if (obj == null)
                            continue;


                        foreach (var head in headerArr)
                        {
                            var name = head.Trim();
                            var val = obj.SelectToken(name)?.ToString();
                            sb.Append(val + seperator);
                        }
                        sb.AppendLine();
                    }
                }


                var csv = sb.ToString();

                var result = new ExecutableResult
                {
                    Result = csv,
                };
                return result;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
