using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Reportz.Scripting.Classes;
using Reportz.Scripting.Interfaces;

namespace Reportz.Plugin.ProjectPlace
{
    public class TimeReportExtractor : IExecutable
    {
        public TimeReportExtractor()
        {
            
        }

        public string Username { get; set; }
        public string Password { get; set; }
        public string Url { get; set; }

        public IExecutableResult Execute(IExecutableArgs args)
        {
            // do stuff...

            var result = new ExecutableResult
            {
                Result = $":: {Username} ::",
            };
            return result;
        }
    }
}
