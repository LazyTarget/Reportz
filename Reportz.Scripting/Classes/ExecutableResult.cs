using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Reportz.Scripting.Interfaces;

namespace Reportz.Scripting.Classes
{
    public class ExecutableResult : IExecutableResult
    {
        public object Result { get; set; }

        public override string ToString()
        {
            if (Result != null)
                return Result?.ToString();
            return "[NULL]";
            //return base.ToString();
        }
    }
}
