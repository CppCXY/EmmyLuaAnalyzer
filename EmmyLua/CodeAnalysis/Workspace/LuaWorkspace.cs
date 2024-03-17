using EmmyLua.CodeAnalysis.Compilation;
using EmmyLua.CodeAnalysis.Document;
using EmmyLua.CodeAnalysis.Workspace.Module;

namespace EmmyLua.CodeAnalysis.Workspace;

public class LuaWorkspace
{
    public LuaFeatures Features { get; }

    private Dictionary<DocumentId, LuaDocument> Documents { get; set; } = new();

    private Dictionary<string, DocumentId> UrlToDocument { get; set; } = new();

    private Dictionary<string, DocumentId> PathToDocument { get; set; } = new();

    private int _idCounter = 1;

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

        foreach (var document in documents)
        {
            document.Id = AllocateId();
        }

        ModuleGraph.AddDocuments(workspace, documents);

        foreach (var document in documents)
        {
            Documents.Add(document.Id, document);
            UrlToDocument.Add(document.Uri, document.Id);
            PathToDocument.Add(document.Path, document.Id);
        }

        Compilation.AddSyntaxTrees(documents.Select(it => (it.Id, it.SyntaxTree)));
    }

    private DocumentId AllocateId()
    {
        return new DocumentId(_idCounter++);
    }

    public LuaDocument? GetDocument(DocumentId id)
    {
        return Documents.GetValueOrDefault(id);
    }

    public LuaDocument? GetDocumentByUri(string uri)
    {
        return UrlToDocument.TryGetValue(uri, out var id) ? GetDocument(id) : null;
    }

    public LuaDocument? GetDocumentByPath(string path)
    {
        return PathToDocument.TryGetValue(path, out var id) ? GetDocument(id) : null;
    }

    public void AddDocumentByUri(string uri, string text)
    {
        var document = LuaDocument.FromUri(uri, text, Features.Language);
        document.Id = AllocateId();
        Documents.Add(document.Id, document);
        UrlToDocument.Add(document.Uri, document.Id);
        PathToDocument.Add(document.Path, document.Id);
        ModuleGraph.AddDocument(document);
        Compilation.AddSyntaxTree(document.Id, document.SyntaxTree);
    }

    public void AddDocument(LuaDocument document)
    {
        if (document.Id.IsVirtual)
        {
            document.Id = AllocateId();
        }

        Documents.Add(document.Id, document);
        if (!document.IsVirtual)
        {
            UrlToDocument.Add(document.Uri, document.Id);
            PathToDocument.Add(document.Path, document.Id);
            ModuleGraph.AddDocument(document);
        }

        Compilation.AddSyntaxTree(document.Id, document.SyntaxTree);
    }

    public void RemoveDocumentByUri(string uri)
    {
        if (UrlToDocument.TryGetValue(uri, out var id))
        {
            RemoveDocument(id);
        }
    }

    public void RemoveDocument(DocumentId id)
    {
        if (Documents.Remove(id, out var document))
        {
            if (!document.IsVirtual)
            {
                UrlToDocument.Remove(document.Uri);
                PathToDocument.Remove(document.Path);
                ModuleGraph.RemoveDocument(document);
            }

            Compilation.RemoveSyntaxTree(id);
        }
    }

    public void UpdateDocument(DocumentId documentId, string text)
    {
        if (Documents.TryGetValue(documentId, out var document))
        {
            var newDocument = document.WithText(text);
            if (!newDocument.IsVirtual)
            {
                ModuleGraph.RemoveDocument(document);
                ModuleGraph.AddDocument(newDocument);
            }

            Documents[documentId] = newDocument;
            Compilation.RemoveSyntaxTree(documentId);
            Compilation.AddSyntaxTree(documentId, newDocument.SyntaxTree);
        }
    }

    public void UpdateDocumentByUri(string uri, string text)
    {
        if (UrlToDocument.TryGetValue(uri, out var id))
        {
            UpdateDocument(id, text);
        }
        else
        {
            AddDocumentByUri(uri, text);
        }
    }

    public void CloseDocument(string uri)
    {
        if (UrlToDocument.TryGetValue(uri, out var id))
        {
            var document = GetDocument(id);
            if (document is not null)
            {
                if (ModuleGraph.GetWorkspace(document).Length == 0)
                {
                    RemoveDocument(id);
                }
            }
        }
    }
}
