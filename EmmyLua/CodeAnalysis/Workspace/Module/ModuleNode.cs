using EmmyLua.CodeAnalysis.Document;

namespace EmmyLua.CodeAnalysis.Workspace.Module;

public class ModuleNode
{
    public Dictionary<string, ModuleNode> Children { get; } = new();

    public DocumentId? DocumentId { get; set; }
}
