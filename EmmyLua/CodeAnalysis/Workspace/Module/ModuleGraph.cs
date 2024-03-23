using System.Text.RegularExpressions;
using EmmyLua.CodeAnalysis.Document;

namespace EmmyLua.CodeAnalysis.Workspace.Module;

public class ModuleGraph(LuaWorkspace workspace)
{
    private Dictionary<string, ModuleNode> WorkspaceModule { get; } = new();

    private Dictionary<DocumentId, ModuleIndex> DocumentIndex { get; } = new();

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

    public void AddDocuments(string workspace, List<LuaDocument> documents)
    {
        workspace = Path.GetFullPath(workspace);
        if (!WorkspaceModule.TryGetValue(workspace, out var root))
        {
            root = new ModuleNode();
            WorkspaceModule.Add(workspace, root);
        }

        foreach (var document in documents)
        {
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
                DocumentIndex.Add(documentId, new ModuleIndex(workspace, modulePath.Replace('/', '.')));
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
        }
    }

    public string GetWorkspace(LuaDocument document)
    {
        var workspace = string.Empty;
        var documentFullPath = Path.GetFullPath(document.Path);
        foreach (var node in WorkspaceModule)
        {
            if (documentFullPath.StartsWith(node.Key))
            {
                workspace = node.Key;
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

            if (node.DocumentId is { } documentId)
            {
                return workspace.GetDocument(documentId);
            }
        }

        return null;
    }
}
