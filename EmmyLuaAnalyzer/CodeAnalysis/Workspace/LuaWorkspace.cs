using EmmyLuaAnalyzer.CodeAnalysis.Compilation;
using EmmyLuaAnalyzer.CodeAnalysis.Workspace.Module;
using EmmyLuaAnalyzer.CodeAnalysis.Syntax.Tree;

namespace EmmyLuaAnalyzer.CodeAnalysis.Workspace;

public class LuaWorkspace
{
    public string WorkspacePath { get; }

    public List<string> ExternalWorkspace { get; } = new();

    public LuaFeatures Features { get; }

    private Dictionary<DocumentId, LuaDocument> Documents { get; set; } = new();

    private Dictionary<string, DocumentId> UrlToDocument { get; set; } = new();

    private Dictionary<string, DocumentId> PathToDocument { get; set; } = new();

    private LuaCompilation Compilation { get; }

    public ModuleGraph ModuleGraph { get; }

    public static LuaWorkspace Create(string workspacePath)
    {
        return Create(workspacePath, new LuaFeatures());
    }

    public static LuaWorkspace Create(string workspacePath, LuaFeatures features)
    {
        var workspace = new LuaWorkspace(workspacePath, features);
        workspace.LoadWorkspace(workspacePath);
        return workspace;
    }

    private LuaWorkspace(string workspacePath, LuaFeatures features)
    {
        WorkspacePath = workspacePath;
        Features = features;
        Compilation = new LuaCompilation(this);
        ModuleGraph = new ModuleGraph(this);
        ModuleGraph.UpdatePattern(features.RequirePattern);
    }

    public void AddExternalWorkspace(string workspacePath)
    {
        ExternalWorkspace.Add(workspacePath);
        LoadWorkspace(workspacePath);
    }

    private void LoadWorkspace(string workspace)
    {
        var files = Directory.GetFiles(workspace, Features.Extensions, SearchOption.AllDirectories)
            .Where(file => !Features.ExcludeFolders.Any(file.Contains));

        var documents =
            new List<LuaDocument>(files.AsParallel().Select(file => LuaDocument.OpenDocument(file, Features.Language)));

        ModuleGraph.AddDocuments(workspace, documents);

        foreach (var document in documents)
        {
            Documents.Add(document.Id, document);
            UrlToDocument.Add(document.Id.Url, document.Id);
            PathToDocument.Add(document.Id.Path, document.Id);
        }

        Compilation.AddSyntaxTrees(documents.Select(it => (it.Id, it.SyntaxTree)));
    }

    public LuaDocument? GetDocument(DocumentId id)
    {
        return Documents.GetValueOrDefault(id);
    }

    public LuaDocument? GetDocument(string url)
    {
        return UrlToDocument.TryGetValue(url, out var id) ? GetDocument(id) : null;
    }
}
