using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            var jsonVar = args?.Arguments?.FirstOrDefault(x => x.Key == "json");

            var success = !string.IsNullOrWhiteSpace(jsonVar?.Value?.ToString());

            var result = new ExecutableResult
            {
                Result = success ? "Successfully posted args" : "Could not parse args",
            };
            return result;
        }
    }
}
