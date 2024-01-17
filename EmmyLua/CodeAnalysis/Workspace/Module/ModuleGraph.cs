using System.Text.RegularExpressions;

namespace EmmyLua.CodeAnalysis.Workspace.Module;

public class ModuleGraph(LuaWorkspace luaWorkspace)
{
    public LuaWorkspace Workspace { get; } = luaWorkspace;

    public Dictionary<string, ModuleNode> WorkspaceModule { get; } = new();

    public Dictionary<DocumentId, ModuleIndex> DocumentIndex { get; } = new();

    public List<Regex> Pattern { get; } = new();

    public void UpdatePattern(List<string> pattern)
    {
        Pattern.Clear();
        foreach (var item in pattern)
        {
            var regexStr = $"^{Regex.Escape(item.Replace('\\', '/')).Replace("\\?", "(.*)")}$";
            Pattern.Add(new Regex(regexStr));
        }
    }

    public void AddDocuments(string workspace, List<LuaDocument> documents)
    {
        foreach (var document in documents)
        {
            AddDocument(workspace, document);
        }
    }

    public void AddDocument(string workspace, LuaDocument document)
    {
        var documentId = document.Id;
        if (documentId.IsVirtual)
        {
            return;
        }

        if (!WorkspaceModule.TryGetValue(workspace, out var root))
        {
            root = new ModuleNode();
            WorkspaceModule.Add(workspace, root);
        }

        // 取得相对于workspace的路径
        var relativePath = Path.GetRelativePath(workspace, documentId.Path);
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

                node.Document = document;
                DocumentIndex.Add(documentId, new ModuleIndex(workspace, modulePath.Replace('/', '.')));
                break;
            }
        }
    }

    public void AddDocument(LuaDocument document)
    {
        AddDocument(GetWorkspace(document.Id), document);
    }

    public void RemoveDocument(DocumentId id)
    {
        RemoveDocument(GetWorkspace(id), id);
    }

    public void RemoveDocument(string workspace, DocumentId documentId)
    {
        if (documentId.IsVirtual)
        {
            return;
        }

        if (!WorkspaceModule.TryGetValue(workspace, out var root))
        {
            return;
        }

        if (DocumentIndex.TryGetValue(documentId, out var moduleIndex))
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

            node.Document = null;
            DocumentIndex.Remove(documentId);
        }
    }

    public string GetWorkspace(DocumentId documentId)
    {
        var workspace = string.Empty;
        if (documentId.IsVirtual)
        {
            return workspace;
        }

        foreach (var pair in WorkspaceModule)
        {
            if (documentId.Path.StartsWith(pair.Key))
            {
                workspace = pair.Key;
                break;
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

            if (node.Document is { } document)
            {
                return document;
            }
        }

        return null;
    }
}
