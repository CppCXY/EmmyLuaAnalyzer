using EmmyLua.CodeAnalysis.Compilation;
using EmmyLua.CodeAnalysis.Compile.Source;
using EmmyLua.CodeAnalysis.Document;
using EmmyLua.CodeAnalysis.Workspace.Module;
using EmmyLua.CodeAnalysis.Syntax.Tree;

namespace EmmyLua.CodeAnalysis.Workspace;

public class LuaWorkspace
{
    public LuaFeatures Features { get; }

    private Dictionary<DocumentId, LuaDocument> Documents { get; set; } = new();

    private Dictionary<string, DocumentId> UrlToDocument { get; set; } = new();

    private Dictionary<string, DocumentId> PathToDocument { get; set; } = new();

    public LuaCompilation Compilation { get; }

    public ModuleGraph ModuleGraph { get; }

    public static LuaWorkspace Create() => Create("", new LuaFeatures());

    public static LuaWorkspace Create(string workspacePath)
    {
        return Create(workspacePath, new LuaFeatures());
    }

    public static LuaWorkspace Create(string workspacePath, LuaFeatures features)
    {
        var workspace = new LuaWorkspace(features);
        if (workspacePath.Length != 0)
        {
            workspace.LoadWorkspace(workspacePath);
        }

        return workspace;
    }

    public LuaWorkspace(LuaFeatures features)
    {
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
        var document = LuaDocument.FromUri(uri, text, Features.Language);
        Documents.Add(document.Id, document);
        UrlToDocument.Add(document.Id.Url, document.Id);
        PathToDocument.Add(document.Id.Path, document.Id);
        ModuleGraph.AddDocument(document);
        Compilation.AddSyntaxTree(document.Id, document.SyntaxTree);
    }

    public void AddDocument(LuaDocument document)
    {
        Documents.Add(document.Id, document);
        if (!document.Id.IsVirtual)
        {
            UrlToDocument.Add(document.Id.Url, document.Id);
            PathToDocument.Add(document.Id.Path, document.Id);
            ModuleGraph.AddDocument(document);
        }

        Compilation.AddSyntaxTree(document.Id, document.SyntaxTree);
    }

    public void RemoveDocument(string uri)
    {
        if (UrlToDocument.TryGetValue(uri, out var id))
        {
            Documents.Remove(id);
            UrlToDocument.Remove(uri);
            PathToDocument.Remove(id.Path);
            ModuleGraph.RemoveDocument(id);
            Compilation.RemoveSyntaxTree(id);
        }
    }

    public void UpdateDocument(string uri, string text)
    {
        if (UrlToDocument.TryGetValue(uri, out var id))
        {
            var document = GetDocument(id);
            if (document is not null)
            {
                var newDocument = document.WithText(text);
                ModuleGraph.RemoveDocument(id);
                ModuleGraph.AddDocument(newDocument);
                Compilation.AddSyntaxTree(id, newDocument.SyntaxTree);
                Documents[id] = newDocument;
            }
            else
            {
                AddDocument(uri, text);
            }
        }
    }


    public void CloseDocument(string uri)
    {
        if (UrlToDocument.TryGetValue(uri, out var id))
        {
            var document = GetDocument(id);
            if (document is not null)
            {
                if (ModuleGraph.GetWorkspace(id).Length == 0)
                {
                    RemoveDocument(id.Url);
                }
            }
        }
    }
}
