using System;
using System.Collections.Generic;
using System.Linq;
using Reportz.Scripting.Interfaces;

namespace Reportz.Scripting.Classes
{
    public class ExecutableResult : IExecutableResult
    {
        public IExecutableArgs Args { get; set; }
        public object Result { get; set; }
        public bool HasResult { get; set; }

        public override string ToString()
        {
            if (Result != null)
                return Result?.ToString();
            return "[NULL]";
            //return base.ToString();
        }

        public object GetValue()
        {
            return Result;
        }
    }
}
