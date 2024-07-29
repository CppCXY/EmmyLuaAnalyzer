using EmmyLua.CodeAnalysis.Document;
using EmmyLua.CodeAnalysis.Syntax.Node;

namespace EmmyLua.CodeAnalysis.Type.Manager;

public class NamespaceOrTypeInfo
{
    public string Name { get; init; } = string.Empty;

    public Dictionary<string, NamespaceOrTypeInfo>? Children { get; set; } = null;

    public TypeInfo.TypeInfo? TypeInfo { get; set; }

    public void Remove(LuaDocumentId documentId)
    {
        if (Children is null)
        {
            return;
        }

        var toBeRemoved = new List<string>();
        foreach (var child in Children.Values)
        {
            if (child.TypeInfo is not null)
            {
                if (child.TypeInfo.Partial)
                {
                    if (child.TypeInfo.IsDefinedInDocument(documentId) && child.TypeInfo.RemovePartial(documentId))
                    {
                        child.TypeInfo = null;
                    }
                }
                else if (child.TypeInfo.MainDocumentId == documentId)
                {
                    child.TypeInfo = null;
                }
            }

            child.Remove(documentId);

            if (child.TypeInfo is null && child.Children is null)
            {
                toBeRemoved.Add(child.Name);
            }
        }

        foreach (var name in toBeRemoved)
        {
            Children.Remove(name);
        }

        if (Children.Count == 0)
        {
            Children = null;
        }
    }

    public void RemoveChildNamespace(string fullName)
    {
        if (fullName == string.Empty)
        {
            return;
        }

        var parts = fullName.Split('.');
        var stack = new Stack<NamespaceOrTypeInfo>();
        var children = Children;
        foreach (var part in parts)
        {
            if (children is null || !children.TryGetValue(part, out var child))
            {
                break;
            }

            stack.Push(child);
        }

        while (stack.Count != 0)
        {
            var child = stack.Pop();
            if (stack.Count == 0)
            {
                Children?.Remove(child.Name);
                break;
            }

            var parent = stack.Peek();
            parent.Children?.Remove(child.Name);
            if (parent.Children?.Count == 0)
            {
                parent.Children = null;
            }

            if (parent.TypeInfo is not null || parent.Children is not null)
            {
                break;
            }
        }
    }

    public NamespaceOrTypeInfo? FindNamespaceOrType(string fullName)
    {
        if (fullName == string.Empty)
        {
            return this;
        }

        var parts = fullName.Split('.');
        return FindNamespaceOrType(parts);
    }

    public NamespaceOrTypeInfo? FindNamespaceOrType(string[] parts)
    {
        var current = this;
        foreach (var part in parts)
        {
            if (current.Children is null)
            {
                return null;
            }

            if (!current.Children.TryGetValue(part, out var child))
            {
                return null;
            }

            current = child;
        }

        return current;
    }

    public NamespaceOrTypeInfo FindOrCreate(string fullName)
    {
        NamespaceOrTypeInfo node = this;
        var parts = fullName.Split('.');
        foreach (var part in parts)
        {
            if (node.Children is null)
            {
                node.Children = new();
            }

            if (!node.Children.TryGetValue(part, out var child))
            {
                child = new NamespaceOrTypeInfo() { Name = part };
                node.Children.Add(part, child);
            }

            node = child;
        }

        return node;
    }

    public bool CreateTypeInfo(SyntaxElementId elementId, NamedTypeKind kind, LuaTypeAttribute attribute)
    {
        if (TypeInfo is not null)
        {
            if (TypeInfo.Partial)
            {
                if (!attribute.HasFlag(LuaTypeAttribute.Partial))
                {
                    return false;
                }

                TypeInfo.DefinedElementIds.Add(elementId);
                TypeInfo.DefinedDocumentIds.Add(elementId.DocumentId);
            }
            else
            {
                return false;
            }
        }
        else
        {
            TypeInfo = new TypeInfo.TypeInfo()
            {
                Name = Name,
                MainDocumentId = elementId.DocumentId,
                Kind = kind,
                Attribute = attribute,
                ResolvedMainDocumentId = kind == NamedTypeKind.Alias,
                DefinedElementIds = [elementId],
                DefinedDocumentIds = [elementId.DocumentId]
            };
        }

        return true;
    }
}
