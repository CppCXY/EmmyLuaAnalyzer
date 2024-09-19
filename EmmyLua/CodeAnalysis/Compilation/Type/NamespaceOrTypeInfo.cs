﻿using EmmyLua.CodeAnalysis.Compilation.Type.TypeCompute;
using EmmyLua.CodeAnalysis.Compilation.Type.TypeInfo;
using EmmyLua.CodeAnalysis.Document;
using EmmyLua.CodeAnalysis.Syntax.Node;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace EmmyLua.CodeAnalysis.Compilation.Type;

public class NamespaceOrTypeInfo
{
    public string Name { get; init; } = string.Empty;

    public Dictionary<string, NamespaceOrTypeInfo>? Children { get; private set; }

    public LuaTypeInfo? TypeInfo { get; private set; }

    public void Remove(LuaDocumentId documentId, LuaTypeManager typeManager)
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
                if (child.TypeInfo.Remove(documentId, typeManager))
                {
                    child.TypeInfo = null;
                }
            }

            child.Remove(documentId, typeManager);

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

    public LuaTypeInfo? CreateTypeInfo(SyntaxElementId elementId, NamedTypeKind kind, LuaTypeAttribute attribute)
    {
        if (TypeInfo is not null)
        {
            if (TypeInfo.Partial)
            {
                if (!attribute.HasFlag(LuaTypeAttribute.Partial))
                {
                    return null;
                }

                TypeInfo.AddDefineId(elementId);
            }
        }
        else
        {
            if (attribute.HasFlag(LuaTypeAttribute.Global))
            {
                TypeInfo = new LuaGlobalTypeInfo(kind, attribute);
            }
            else if (attribute.HasFlag(LuaTypeAttribute.Partial))
            {
                TypeInfo = new LuaPartialTypeInfo(kind, attribute);
            }
            else
            {
                TypeInfo = new LuaLocalTypeInfo(elementId, kind, attribute);
            }

            TypeInfo.AddDefineId(elementId);
        }

        return null;
    }

    public LuaComputerTypeInfo? CreateComputerTypeInfo(SyntaxElementId elementId, LuaDocTagAliasSyntax tagAlias)
    {
        if (TypeInfo is not null)
        {
            return null;
        }

        var genericList = new List<string>();
        if (tagAlias.GenericDeclareList?.Params is { } genericParamSyntaxes)
        {
            foreach (var genericParamSyntax in genericParamSyntaxes)
            {
                if (genericParamSyntax.Name?.RepresentText is { } name)
                {
                    genericList.Add(name);
                }
            }
        }

        var typeSyntax = tagAlias.Type;
        if (typeSyntax is null)
        {
            return null;
        }

        var computer = TypeComputer.Compile(genericList, typeSyntax);
        var typeInfo = new LuaComputerTypeInfo(elementId, computer);
        TypeInfo = typeInfo;
        return typeInfo;
    }
}