using System;

namespace Reportz.Scripting.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public class ScriptElementAliasAttribute : Attribute
    {
        public ScriptElementAliasAttribute(string alias)
        {
            Alias = alias;
        }

        public string Alias { get; private set; }
    }
}
