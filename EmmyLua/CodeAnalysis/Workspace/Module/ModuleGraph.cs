using System.Text.RegularExpressions;
using EmmyLua.CodeAnalysis.Document;

namespace EmmyLua.CodeAnalysis.Workspace.Module;

public class ModuleGraph(LuaWorkspace luaWorkspace)
{
    private LuaWorkspace Workspace { get; } = luaWorkspace;

    private Dictionary<string, ModuleNode> WorkspaceModule { get; } = new();

    private Dictionary<LuaDocumentId, ModuleIndex> DocumentIndex { get; } = new();

    public Dictionary<string, List<LuaDocumentId>> ModuleNameToDocumentId { get; } = new();

    private List<Regex> Pattern { get; } = new();

    public void UpdatePattern(List<string> pattern)
    {
        Pattern.Clear();
        foreach (var item in pattern)
        {
            var regexStr = $"^{Regex.Escape(item.Replace('\\', '/')).Replace("\\?", "(.*)")}$";
            Pattern.Add(new Regex(regexStr));
        }
    }

    public void AddPackageRoot(string packageRoot)
    {
        packageRoot = Path.GetFullPath(packageRoot);
        if (!WorkspaceModule.ContainsKey(packageRoot))
        {
            WorkspaceModule.Add(packageRoot, new ModuleNode());
        }
    }

    public void AddDocuments(List<LuaDocument> documents)
    {
        foreach (var document in documents)
        {
            var workspace = GetWorkspace(document);
            if (!WorkspaceModule.TryGetValue(workspace, out var root))
            {
                root = new ModuleNode();
                WorkspaceModule.Add(workspace, root);
            }

            AddDocument(root, workspace, document);
        }
    }

    public void AddDocument(ModuleNode root, string workspace, LuaDocument document)
    {
        var documentId = document.Id;

        // 取得相对于workspace的路径
        var relativePath = Path.GetRelativePath(workspace, document.Path);
        var normalPath = relativePath.Replace('\\', '/');
        foreach (var regex in Pattern)
        {
            var match = regex.Match(normalPath);
            if (match.Success)
            {
                var modulePath = match.Groups[1].Value;
                var modulePaths = modulePath.Split('/');
                var node = root;
                foreach (var path in modulePaths)
                {
                    if (!node.Children.TryGetValue(path, out var child))
                    {
                        child = new ModuleNode();
                        node.Children.Add(path, child);
                    }

                    node = child;
                }

                node.DocumentId = documentId;
                var requiredModulePath = modulePath.Replace('/', '.');
                var name = requiredModulePath;
                var lastDotIndex = requiredModulePath.LastIndexOf('.');
                if (lastDotIndex >= 0 && lastDotIndex < requiredModulePath.Length - 1)
                {
                    name = requiredModulePath[(lastDotIndex + 1)..];
                }

                var moduleIndex = new ModuleIndex(documentId, name, workspace, requiredModulePath);
                DocumentIndex.Add(documentId, moduleIndex);

                if (!ModuleNameToDocumentId.TryGetValue(name, out var documentIds))
                {
                    documentIds = new List<LuaDocumentId> { documentId };
                    ModuleNameToDocumentId.Add(name, documentIds);
                }
                else
                {
                    documentIds.Add(documentId);
                }

                break;
            }
        }
    }

    public void AddDocument(LuaDocument document)
    {
        var workspace = GetWorkspace(document);
        if (workspace.Length == 0)
        {
            return;
        }

        if (!WorkspaceModule.TryGetValue(workspace, out var root))
        {
            return;
        }

        AddDocument(root, workspace, document);
    }

    public void RemoveDocument(LuaDocument document)
    {
        var workspace = string.Empty;
        if (DocumentIndex.TryGetValue(document.Id, out var moduleIndex))
        {
            workspace = moduleIndex.Workspace;
        }

        if (workspace.Length == 0)
        {
            return;
        }

        if (!WorkspaceModule.TryGetValue(workspace, out var root))
        {
            return;
        }

        RemoveDocument(root, document);
    }

    private void RemoveDocument(ModuleNode root, LuaDocument document)
    {
        if (DocumentIndex.TryGetValue(document.Id, out var moduleIndex))
        {
            var modulePaths = moduleIndex.ModulePath.Split('.');
            var node = root;
            foreach (var path in modulePaths)
            {
                if (!node.Children.TryGetValue(path, out var child))
                {
                    return;
                }

                node = child;
            }

            node.DocumentId = null;
            DocumentIndex.Remove(document.Id);
            if (ModuleNameToDocumentId.TryGetValue(moduleIndex.Name, out var documentIds))
            {
                documentIds.Remove(document.Id);
                if (documentIds.Count == 0)
                {
                    ModuleNameToDocumentId.Remove(moduleIndex.Name);
                }
            }
        }
    }

    public string GetWorkspace(LuaDocument document)
    {
        var workspace = string.Empty;
        var documentFullPath = Path.GetFullPath(document.Path);
        foreach (var node in WorkspaceModule)
        {
            if (node.Key.Length > workspace.Length && documentFullPath.StartsWith(node.Key))
            {
                workspace = node.Key;
            }
        }

        return workspace;
    }

    public LuaDocument? FindModule(string modulePath)
    {
        var modulePaths = modulePath.Split('.');

        foreach (var moduleNode in WorkspaceModule)
        {
            var node = moduleNode.Value;
            foreach (var path in modulePaths)
            {
                if (!node.Children.TryGetValue(path, out var child))
                {
                    break;
                }

                node = child;
            }

            if (node.DocumentId is { } documentId)
            {
                return Workspace.GetDocument(documentId);
            }
        }

        return null;
    }

    public readonly struct ModuleInfo(string name, string uri, bool isFile, LuaDocumentId? documentId)
    {
        public string Name { get; } = name;

        public string Uri { get; } = uri;

        public bool IsFile { get; } = isFile;

        public LuaDocumentId? DocumentId { get; } = documentId;
    }

    public List<ModuleInfo> GetCurrentModuleNames(string modulePath)
    {
        var moduleInfos = new List<ModuleInfo>();
        var parts = modulePath.Split('.');
        if (parts.Length <= 1)
        {
            foreach (var moduleNode in WorkspaceModule)
            {
                var node = moduleNode.Value;
                foreach (var child in node.Children)
                {
                    var uri = string.Empty;
                    if (child.Value.DocumentId.HasValue)
                    {
                        var document = Workspace.GetDocument(child.Value.DocumentId.Value);
                        uri = document?.Uri ?? string.Empty;
                    }

                    if (uri.Length == 0)
                    {
                        uri = new Uri(Path.Join(moduleNode.Key, child.Key)).AbsoluteUri;
                    }

                    moduleInfos.Add(new ModuleInfo(child.Key, uri, child.Value.DocumentId.HasValue,
                        child.Value.DocumentId));
                }
            }

            return moduleInfos;
        }

        parts = parts[..^1];
        var moduleBasePath = string.Join('/', parts);
        foreach (var moduleNode in WorkspaceModule)
        {
            var node = moduleNode.Value;
            foreach (var path in parts)
            {
                if (!node.Children.TryGetValue(path, out var child))
                {
                    node = null;
                    break;
                }

                node = child;
            }

            if (node is not null)
            {
                foreach (var child in node.Children)
                {
                    var uri = string.Empty;
                    if (child.Value.DocumentId.HasValue)
                    {
                        var document = Workspace.GetDocument(child.Value.DocumentId.Value);
                        uri = document?.Uri ?? string.Empty;
                    }

                    if (uri.Length == 0)
                    {
                        uri = new Uri(Path.Join(moduleNode.Key, moduleBasePath, child.Key)).AbsoluteUri;
                    }

                    moduleInfos.Add(new ModuleInfo(child.Key, uri, child.Value.DocumentId.HasValue,
                        child.Value.DocumentId));
                }
            }
        }

        return moduleInfos;
    }

    public List<ModuleIndex> GetAllModules()
    {
        return DocumentIndex.Values.ToList();
    }

    public ModuleIndex? GetModuleInfo(LuaDocumentId documentId)
    {
        DocumentIndex.TryGetValue(documentId, out var moduleInfo);
        return moduleInfo;
    }
}
