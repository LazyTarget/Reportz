using System;

namespace Reportz.Helpers.Excel.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    internal class XlsxScriptElementAliasAttribute : Attribute
    {
        public XlsxScriptElementAliasAttribute(string alias)
        {
            Alias = alias;
        }

        public string Alias { get; private set; }
    }
}
