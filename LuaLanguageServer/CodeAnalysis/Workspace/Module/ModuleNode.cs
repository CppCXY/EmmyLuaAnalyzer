namespace LuaLanguageServer.CodeAnalysis.Workspace.Module;

public class ModuleNode
{
    public Dictionary<string, ModuleNode> Children { get; } = new();

    public LuaDocument? Document { get; set; }
}
