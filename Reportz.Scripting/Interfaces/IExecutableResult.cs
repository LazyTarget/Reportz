namespace Reportz.Scripting.Interfaces
{
    public interface IExecutableResult : IHasValue
    {
        IExecutableArgs Args { get; }
        object Result { get; }
    }
}
