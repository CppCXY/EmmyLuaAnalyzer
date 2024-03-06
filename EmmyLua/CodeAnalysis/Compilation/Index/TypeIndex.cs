﻿using EmmyLua.CodeAnalysis.Compilation.Type;
using EmmyLua.CodeAnalysis.Document;

namespace EmmyLua.CodeAnalysis.Compilation.Index;

public class TypeIndex(LuaCompilation compilation)
{
    private Dictionary<TypeOperatorKind, IndexEntry<TypeOperator>> TypeOperators { get; } = new();

    private Dictionary<string, IndexEntry<TypeFeature>> Features { get; } = new();

    public void AddTypeOperator(DocumentId documentId, TypeOperator typeOperator)
    {
        if (!TypeOperators.TryGetValue(typeOperator.Kind, out var entry))
        {
            entry = new IndexEntry<TypeOperator>();
            TypeOperators.Add(typeOperator.Kind, entry);
        }

        entry.Add(documentId, typeOperator);
    }

    public void AddFeature(DocumentId documentId, string name, TypeFeature feature)
    {
        if (!Features.TryGetValue(name, out var entry))
        {
            entry = new IndexEntry<TypeFeature>();
            Features.Add(name, entry);
        }

        entry.Add(documentId, feature);
    }

    public void Remove(DocumentId documentId)
    {
        RemoveOperator(documentId);
        RemoveFeatures(documentId);
    }

    private void RemoveOperator(DocumentId documentId)
    {
        var waitRemove = new List<TypeOperatorKind>();
        foreach (var (key, entry) in TypeOperators)
        {
            entry.Remove(documentId);
            if (entry.Files.Count == 0)
            {
                waitRemove.Add(key);
            }
        }

        foreach (var key in waitRemove)
        {
            TypeOperators.Remove(key);
        }
    }

    private void RemoveFeatures(DocumentId documentId)
    {
        var waitRemove = new List<string>();
        foreach (var (key, entry) in Features)
        {
            entry.Remove(documentId);
            if (entry.Files.Count == 0)
            {
                waitRemove.Add(key);
            }
        }

        foreach (var key in waitRemove)
        {
            Features.Remove(key);
        }
    }

    public BinaryOperator? GetBestMatchedBinaryOperator(TypeOperatorKind kind, LuaType left, LuaType right)
    {
        if (!TypeOperators.TryGetValue(kind, out var entry))
        {
            return null;
        }

        // var bestMatched = entry.Files.Values.SelectMany(it => it.Elements)
        //     .OfType<BinaryOperator>()
        //     .Where(it => it.Left.Equals(left) && it.Right.Equals(right))
        //     .OrderByDescending(it => it.Left, it => it.Right)
        //     .FirstOrDefault();
        //
        // return bestMatched;
        throw new NotImplementedException();
    }

    public UnaryOperator? GetBestMatchedUnaryOperator(TypeOperatorKind kind, LuaType type)
    {
        if (!TypeOperators.TryGetValue(kind, out var entry))
        {
            return null;
        }

        // var bestMatched = entry.Files.Values.SelectMany(it => it.Elements)
        //     .OfType<TypeOperator>()
        //     .Where(it => it.Left.Equals(type))
        //     .OrderByDescending(it => it.Left)
        //     .FirstOrDefault();
        //
        // return bestMatched;
        throw new NotImplementedException();
    }

    public IndexOperator? GetBestMatchedIndexOperator(LuaType type, LuaType key)
    {
        if (!TypeOperators.TryGetValue(TypeOperatorKind.Index, out var entry))
        {
            return null;
        }

        // var bestMatched = entry.Files.Values.SelectMany(it => it.Elements)
        //     .OfType<IndexOperator>()
        //     .Where(it => it.Type.Equals(type) && it.Key.Equals(key))
        //     .OrderByDescending(it => it.Type, it => it.Key)
        //     .FirstOrDefault();
        //
        // return bestMatched;
        throw new NotImplementedException();
    }

    public TypeFeature GetFeature(string name)
    {
        if (!Features.TryGetValue(name, out var entry))
        {
            return TypeFeature.Class;
        }

        return entry.Files.Values.SelectMany(it => it)
            .OrderByDescending(it => it)
            .FirstOrDefault();
    }
}