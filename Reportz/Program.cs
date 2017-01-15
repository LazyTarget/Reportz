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
            var script = scriptFactory.Parse(text);

            Console.WriteLine("Executing script...");
            Console.WriteLine();

            IExecutableArgs arg = null;
            var result = script.Execute(arg);

            Console.WriteLine();
            Console.WriteLine("::ExecutableResult:: ");
            Console.WriteLine(result?.Result);

            Console.ReadLine();
        }
    }
}
