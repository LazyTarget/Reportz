using System;
using Reportz.Scripting.Classes;
using Reportz.Scripting.Interfaces;

namespace Reportz.Scripting
{
    public static class ScriptExtensions
    {
        public static T GetVarValue<T>(this VariableScope scope, string key)
        {
            var variable = scope.GetVariable(key);
            var obj = (T)variable.Value;
            return obj;
        }

        public static T GetVarValueOrDefault<T>(this VariableScope scope, string key)
        {
            var variable = scope.GetVariable(key);
            if (variable != null)
            {
                var obj = (T) variable.Value;
                return obj;
            }
            return default(T);
        }


        public static IVariable CreateSimpleVariable(this VariableScope scope, string key, object value)
        {
            var variable = new SimpleVariable(key, value);
            scope.SetVariable(variable);
            return variable;
        }

        public static IVariable CreateLazyVariable(this VariableScope scope, string key, Func<object> func)
        {
            var variable = new LazyVariable(key, func);
            scope.SetVariable(variable);
            return variable;
        }



        public static IScriptContext GetScriptContext(this VariableScope scope)
        {
            var obj = scope.GetVariable("$$Context")?.Value;
            var ctx = obj as IScriptContext;
            return ctx;
        }

        public static IScriptDocument GetScriptDocument(this VariableScope scope)
        {
            var obj = scope.GetVariable("$$Document")?.Value;
            var doc = obj as IScriptDocument;
            return doc;
        }
    }
}