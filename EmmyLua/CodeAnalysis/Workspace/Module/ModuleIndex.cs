using EmmyLua.CodeAnalysis.Document;

namespace EmmyLua.CodeAnalysis.Workspace.Module;

public class ModuleIndex(LuaDocumentId documentId, string name, string workspace, string modulePath)
{
    public string Name { get; } = name;

    public LuaDocumentId DocumentId { get; } = documentId;

    public string Workspace { get; } = workspace;

    public string ModulePath { get; } = modulePath;
}
