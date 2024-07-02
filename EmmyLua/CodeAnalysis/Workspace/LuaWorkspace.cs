using EmmyLua.CodeAnalysis.Common;
using EmmyLua.CodeAnalysis.Compilation;
using EmmyLua.CodeAnalysis.Document;
using EmmyLua.CodeAnalysis.Workspace.Module;

namespace EmmyLua.CodeAnalysis.Workspace;

public class LuaWorkspace
{
    public string MainWorkspace { get; set; } = string.Empty;

    private LuaFeatures _features;

    public LuaFeatures Features
    {
        get => _features;
        set
        {
            _features = value;
            ModuleManager.UpdatePattern(_features.RequirePattern);
        }
    }

    private Dictionary<LuaDocumentId, LuaDocument> Documents { get; set; } = new();

    // Windows is case-insensitive, so we need to use a case-insensitive comparer
    class CaseInsensitiveComparer : IEqualityComparer<string>
    {
        public bool Equals(string? x, string? y)
        {
            return string.Equals(x, y, StringComparison.OrdinalIgnoreCase);
        }

        public int GetHashCode(string obj)
        {
            return obj.ToLower().GetHashCode();
        }
    }

    private Dictionary<string, LuaDocumentId> UrlToDocument { get; set; } = new(new CaseInsensitiveComparer());

    private Dictionary<string, LuaDocumentId> PathToDocument { get; set; } = new(new CaseInsensitiveComparer());

    public IEnumerable<LuaDocument> AllDocuments => Documents.Values;

    private int _idCounter = 1;

    public LuaWorkspaceMonitor? Monitor { get; set; }

    public LuaCompilation Compilation { get; }

    public ModuleManager ModuleManager { get; }

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
            workspace.LoadMainWorkspace(workspacePath);
        }

        return workspace;
    }

    public static LuaWorkspace CleanCreate()
    {
        var features = new LuaFeatures()
        {
            InitStdLib = false
        };
        return new LuaWorkspace(features);
    }

    public LuaWorkspace(LuaFeatures features)
    {
        _features = features;
        Compilation = new LuaCompilation(this);
        ModuleManager = new ModuleManager(this);
        ModuleManager.UpdatePattern(features.RequirePattern);
        if (features.InitStdLib)
        {
            InitStdLib();
        }
    }

    public void InitStdLib()
    {
        var stdLib = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "std");
        LoadWorkspace(stdLib);
    }

    private IEnumerable<string> CollectFiles(string directory)
    {
        if (!Directory.Exists(directory))
        {
            return Array.Empty<string>();
        }

        var excludeFolders = Features.ExcludeFolders
            .Select(it => Path.Combine(directory, it.Trim('\\', '/')))
            .Select(Path.GetFullPath)
            .ToList();
        return Features.Extensions
            .SelectMany(it => Directory.GetFiles(directory, it, SearchOption.AllDirectories))
            .Select(Path.GetFullPath)
            .Where(file => !excludeFolders.Any(filter => file.StartsWith(filter, StringComparison.OrdinalIgnoreCase)));
    }

    /// this will load all third libraries and workspace files
    public void LoadMainWorkspace(string workspace)
    {
        MainWorkspace = workspace;
        Monitor?.OnStartLoadWorkspace();
        var thirdPartyRoots = Features.ThirdPartyRoots;
        var files = new List<string>();
        foreach (var thirdPartyRoot in thirdPartyRoots)
        {
            files.AddRange(CollectFiles(thirdPartyRoot));
            ModuleManager.AddPackageRoot(thirdPartyRoot);
        }

        files.AddRange(CollectFiles(workspace));
        ModuleManager.AddPackageRoot(workspace);
        foreach (var workspaceRoot in Features.WorkspaceRoots)
        {
            ModuleManager.AddPackageRoot(workspaceRoot);
        }

        var documents =
            new List<LuaDocument>(files.AsParallel().Select(
                file => LuaDocument.FromPath(file, ReadFile(file), Features.Language)));

        foreach (var document in documents)
        {
            if (!PathToDocument.TryGetValue(document.Path, out var id))
            {
                document.Id = AllocateId();
                Documents.Add(document.Id, document);
            }
            else
            {
                document.Id = id;
                Documents[document.Id] = document;
            }

            UrlToDocument[document.Uri] = document.Id;
            PathToDocument[document.Path] = document.Id;
        }

        // for parallel
        var syntaxTrees = documents
            .AsParallel()
            .Select(it => (it.Id, it.SyntaxTree))
            .ToList();

        ModuleManager.AddDocuments(documents);
        Compilation.AddSyntaxTrees(syntaxTrees);
        Monitor?.OnFinishLoadWorkspace();
    }

    public void LoadWorkspace(string workspace)
    {
        Monitor?.OnStartLoadWorkspace();
        var files = CollectFiles(workspace).ToList();
        var documents =
            files.AsParallel().Select(file => LuaDocument.OpenDocument(file, Features.Language)).ToList();
        ModuleManager.AddPackageRoot(workspace);
        foreach (var document in documents)
        {
            if (!PathToDocument.TryGetValue(document.Path, out var id))
            {
                document.Id = AllocateId();
                Documents.Add(document.Id, document);
            }
            else
            {
                document.Id = id;
                Documents[document.Id] = document;
            }

            UrlToDocument[document.Uri] = document.Id;
            PathToDocument[document.Path] = document.Id;
        }

        ModuleManager.AddDocuments(documents);
        Compilation.AddSyntaxTrees(documents.Select(it => (it.Id, it.SyntaxTree)));
        Monitor?.OnFinishLoadWorkspace();
    }

    private LuaDocumentId AllocateId()
    {
        return new LuaDocumentId(_idCounter++);
    }

    public LuaDocument? GetDocument(LuaDocumentId id)
    {
        return Documents.GetValueOrDefault(id);
    }

    public LuaDocumentId? GetDocumentIdByUri(string uri)
    {
        return UrlToDocument.GetValueOrDefault(uri);
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
        Documents[document.Id] = document;
        UrlToDocument[document.Uri] = document.Id;
        PathToDocument[document.Path] = document.Id;
        ModuleManager.AddDocument(document);
        Compilation.AddSyntaxTree(document.Id, document.SyntaxTree);
    }

    public void AddDocument(LuaDocument document)
    {
        if (document.Id.IsVirtual)
        {
            document.Id = AllocateId();
        }

        document.OpenState = OpenState.Opened;
        Documents.Add(document.Id, document);
        if (!document.IsVirtual)
        {
            UrlToDocument.Add(document.Uri, document.Id);
            PathToDocument.Add(document.Path, document.Id);
            ModuleManager.AddDocument(document);
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

    public void RemoveDocument(LuaDocumentId id)
    {
        if (Documents.Remove(id, out var document))
        {
            if (!document.IsVirtual)
            {
                UrlToDocument.Remove(document.Uri);
                PathToDocument.Remove(document.Path);
                ModuleManager.RemoveDocument(document);
            }

            Compilation.RemoveSyntaxTree(id);
        }
    }

    public void UpdateDocument(LuaDocumentId documentId, string text)
    {
        if (Documents.TryGetValue(documentId, out var document))
        {
            document.OpenState = OpenState.Opened;
            document.ReplaceText(text);
            Compilation.RemoveSyntaxTree(documentId);
            Compilation.AddSyntaxTree(documentId, document.SyntaxTree);
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
                document.OpenState = OpenState.Closed;
                if (ModuleManager.GetWorkspace(document).Length == 0)
                {
                    RemoveDocument(id);
                }
            }
        }
    }

    public IDocument? GetDocument(int id)
    {
        return GetDocument(new LuaDocumentId(id));
    }

    // 通过文件路径和设置中的编码读取文件
    public string ReadFile(string path)
    {
        return File.ReadAllText(path, Features.Encoding);
    }
}
