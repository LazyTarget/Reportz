namespace Reportz.Scripting.Interfaces
{
    public interface IScriptContext
    {
        IScriptScope ScriptScope { get; }
        Lux.IO.IFileSystem FileSystem { get; }
    }
}
