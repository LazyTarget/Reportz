using Reportz.Scripting.Classes;

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
    }
}