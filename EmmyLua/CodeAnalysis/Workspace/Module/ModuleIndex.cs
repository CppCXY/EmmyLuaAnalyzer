namespace EmmyLua.CodeAnalysis.Workspace.Module;

public class ModuleIndex(string workspace, string modulePath)
{
    public string Workspace { get; } = workspace;

    public string ModulePath { get; } = modulePath;
}
