using LuaLanguageServer.CodeAnalysis.Compilation;
using LuaLanguageServer.CodeAnalysis.Syntax.Tree;

namespace LuaLanguageServer.CodeAnalysis.Workspace;

public class LuaWorkspace
{
    public string WorkspacePath { get; }

    public List<string> ExternalWorkspace { get; }

    public LuaFeatures Features { get; }

    private Dictionary<DocumentId, LuaDocument> _documents;

    private Dictionary<string, DocumentId> _urlToDocument;

    private Dictionary<string, LuaDocument> _pathToDocument;

    private LuaCompilation _compilation;

    public LuaCompilation Compilation => _compilation;

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
        ExternalWorkspace = new List<string>();
        _documents = new Dictionary<DocumentId, LuaDocument>();
        _urlToDocument = new Dictionary<string, DocumentId>();
        _pathToDocument = new Dictionary<string, LuaDocument>();
        _compilation = new LuaCompilation(this);
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

        _documents = _documents.Concat(documents.ToDictionary(it => it.Id, it => it))
            .ToDictionary(it => it.Key, it => it.Value);

        _urlToDocument = _urlToDocument.Concat(documents.ToDictionary(it => it.Id.Url, it => it.Id))
            .ToDictionary(it => it.Key, it => it.Value);

        _pathToDocument = _pathToDocument.Concat(documents.ToDictionary(it => it.Id.Path, it => it))
            .ToDictionary(it => it.Key, it => it.Value);

        _compilation.AddSyntaxTrees(documents.Select(it => (it.Id, it.SyntaxTree)));
    }

    public LuaDocument? GetDocument(DocumentId id)
    {
        return _documents.TryGetValue(id, out var document) ? document : null;
    }

    public LuaDocument? GetDocument(string url)
    {
        return _urlToDocument.TryGetValue(url, out var id) ? GetDocument(id) : null;
    }

    public LuaDocument? FindModule(string modulePath)
    {
        modulePath = modulePath.Replace('.', '/');
        var modulePaths = Features.RequirePattern
            .Select(it => Path.Combine(WorkspacePath, it.Replace("?", modulePath))).ToList();
        foreach (var module in modulePaths)
        {
            if (_pathToDocument.TryGetValue(module, out var document))
            {
                return document;
            }
        }

        return null;
    }
}
