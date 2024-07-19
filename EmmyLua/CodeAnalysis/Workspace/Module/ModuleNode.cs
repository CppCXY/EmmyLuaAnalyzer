using EmmyLua.CodeAnalysis.Document;

namespace EmmyLua.CodeAnalysis.Workspace.Module;

public class ModuleNode
{
    public Dictionary<string, ModuleNode> Children { get; } = new();

    public LuaDocumentId? DocumentId { get; private set; }

    public void RemoveModule(string modulePath)
    {
        var modulePaths = modulePath.Split('.');
        var node = this;
        var removeStack = new Stack<(string, ModuleNode)>();
        foreach (var path in modulePaths)
        {
            if (!node.Children.TryGetValue(path, out var child))
            {
                return;
            }

            node = child;
            removeStack.Push((path, node));
        }

        node.DocumentId = null;
        while (removeStack.Count > 0)
        {
            var (path, child) = removeStack.Pop();
            if (child.Children.Count == 0 && !child.DocumentId.HasValue)
            {
                var parent = removeStack.Count > 0 ? removeStack.Peek().Item2 : this;
                parent.Children.Remove(path);
            }
            else
            {
                break;
            }
        }
    }

    public void AddModule(IEnumerable<string> modulePaths, LuaDocumentId documentId)
    {
        var node = this;
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
    }


    public LuaDocumentId? FindModule(string modulePath)
    {
        var modulePaths = modulePath.Split('.');
        var node = this;
        foreach (var path in modulePaths)
        {
            if (!node.Children.TryGetValue(path, out var child))
            {
                return null;
            }

            node = child;
        }

        return node.DocumentId;
    }
}
