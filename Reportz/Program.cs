using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Reportz.Scripting.Classes;
using Reportz.Scripting.Interfaces;

namespace Reportz
{
    class Program
    {
        static void Main(string[] args)
        {
            Run(args);
        }

        private static void Run(string[] args)
        {
            var scriptFile = args.ElementAt(0);
            var text = File.ReadAllText(scriptFile);

            var scriptFactory = new ScriptParser();
            var scriptDoc = scriptFactory.ParseDocument(text);

            Console.WriteLine("Executing script doc...");
            Console.WriteLine();

            IExecutableArgs arg = null;
            arg = arg ?? new ExecutableArgs { Scope = new VariableScope() };
            var result = scriptDoc.Execute(arg);

            Console.WriteLine();
            Console.WriteLine("::ExecutableResult:: ");
            Console.WriteLine(result?.Result);

            Console.ReadLine();
        }
    }
}
