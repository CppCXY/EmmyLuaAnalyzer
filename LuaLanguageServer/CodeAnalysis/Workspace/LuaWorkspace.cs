using LuaLanguageServer.CodeAnalysis.Compilation;
using LuaLanguageServer.CodeAnalysis.Syntax.Tree;

namespace LuaLanguageServer.CodeAnalysis.Workspace;

public class LuaWorkspace
{
    public string WorkspacePath { get; }

    public LuaFeatures Features { get; }

    private Dictionary<DocumentId, LuaDocument> _documents;

    private Dictionary<string, DocumentId> _urlToDocument;

    private LuaCompilation _compilation;

    public LuaCompilation Compilation => _compilation;

    public static LuaWorkspace Create(string workspacePath)
    {
        return Create(workspacePath, new LuaFeatures());
    }

    public static LuaWorkspace Create(string workspacePath, LuaFeatures features)
    {
        var workspace = new LuaWorkspace(workspacePath, features);
        workspace.LoadWorkspace();
        return workspace;
    }

    private LuaWorkspace(string workspacePath, LuaFeatures features)
    {
        WorkspacePath = workspacePath;
        Features = features;
        _documents = new Dictionary<DocumentId, LuaDocument>();
        _urlToDocument = new Dictionary<string, DocumentId>();
        _compilation = new LuaCompilation(this);
    }

    private void LoadWorkspace()
    {
        var files = Directory.GetFiles(WorkspacePath, Features.Extensions, SearchOption.AllDirectories);

        var documents =
            new List<LuaDocument>(files.AsParallel().Select(file => LuaDocument.OpenDocument(file, Features.Language)));

        _documents = documents.ToDictionary(it => it.Id, it => it);

        _urlToDocument = documents.ToDictionary(it => it.Id.Url, it => it.Id);

        _compilation = _compilation.AddSyntaxTrees(
            documents.AsParallel().Select(document => LuaSyntaxTree.Create(document.Source)));
    }

    public LuaDocument? GetDocument(DocumentId id)
    {
        return _documents.TryGetValue(id, out var document) ? document : null;
    }

    public LuaDocument? GetDocument(string url)
    {
        return _urlToDocument.TryGetValue(url, out var id) ? GetDocument(id) : null;
    }
}
