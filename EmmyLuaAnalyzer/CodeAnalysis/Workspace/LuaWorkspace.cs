using EmmyLuaAnalyzer.CodeAnalysis.Compilation;
using EmmyLuaAnalyzer.CodeAnalysis.Workspace.Module;
using EmmyLuaAnalyzer.CodeAnalysis.Syntax.Tree;

namespace EmmyLuaAnalyzer.CodeAnalysis.Workspace;

public class LuaWorkspace
{
    public string WorkspacePath { get; }

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
        if (workspacePath.Length != 0)
        {
            workspace.LoadWorkspace(workspacePath);
        }

        return workspace;
    }

    public LuaWorkspace(string workspacePath, LuaFeatures features)
    {
        WorkspacePath = workspacePath;
        Features = features;
        Compilation = new LuaCompilation(this);
        ModuleGraph = new ModuleGraph(this);
        ModuleGraph.UpdatePattern(features.RequirePattern);
    }

    public void LoadWorkspace(string workspace)
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

    public void AddDocument(string uri, string text)
    {
        var document = LuaDocument.From(uri, text, Features.Language);
        Documents.Add(document.Id, document);
        UrlToDocument.Add(document.Id.Url, document.Id);
        PathToDocument.Add(document.Id.Path, document.Id);
        Compilation.AddSyntaxTree(document.Id, document.SyntaxTree);
    }

    public void UpdateDocument(string uri, string text)
    {
        if (UrlToDocument.TryGetValue(uri, out var id))
        {
            var document = GetDocument(id);
            if (document is not null)
            {
                var newDocument = document.WithText(text);
                Compilation.AddSyntaxTree(id, newDocument.SyntaxTree);
                Documents[id] = newDocument;
            }
            else
            {
                AddDocument(uri, text);
            }
        }
    }
}
