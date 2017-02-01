using System;

namespace Reportz.Scripting.Interfaces
{
    public interface IVariable : IExecutable, ICloneable
    {
        string Key { get; }
        object Value { get; set; }
        Type Type { get; }
    }
}
