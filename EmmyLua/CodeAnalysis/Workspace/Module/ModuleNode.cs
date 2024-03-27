using EmmyLua.CodeAnalysis.Document;

namespace EmmyLua.CodeAnalysis.Workspace.Module;

public class ModuleNode
{
    public Dictionary<string, ModuleNode> Children { get; } = new();

    public LuaDocumentId? DocumentId { get; set; }
}
