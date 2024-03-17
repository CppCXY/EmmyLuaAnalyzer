using EmmyLua.CodeAnalysis.Compilation.Type;
using EmmyLua.CodeAnalysis.Document;

namespace EmmyLua.CodeAnalysis.Compilation.Index;

public class TypeOperatorStorage
{
    private Dictionary<TypeOperatorKind, IndexEntry<TypeOperator>> TypeOperators { get; } = new();

    public void AddTypeOperator(DocumentId documentId, TypeOperator typeOperator)
    {
        if (!TypeOperators.TryGetValue(typeOperator.Kind, out var entry))
        {
            entry = new IndexEntry<TypeOperator>();
            TypeOperators.Add(typeOperator.Kind, entry);
        }

        entry.Add(documentId, typeOperator);
    }

    public void Remove(DocumentId documentId)
    {
        RemoveOperator(documentId);
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

    public BinaryOperator? GetBestMatchedBinaryOperator(TypeOperatorKind kind, LuaType left, LuaType right)
    {
        if (!TypeOperators.TryGetValue(kind, out var entry))
        {
            return null;
        }

        var bestMatched = entry.Files.Values
            .SelectMany(it => it)
            .OfType<BinaryOperator>()
            .FirstOrDefault(it => it.Left.Equals(left) && it.Right.Equals(right));

        return bestMatched;
    }

    public UnaryOperator? GetBestMatchedUnaryOperator(TypeOperatorKind kind, LuaType type)
    {
        if (!TypeOperators.TryGetValue(kind, out var entry))
        {
            return null;
        }

        var bestMatched = entry.Files.Values
            .SelectMany(it => it)
            .OfType<UnaryOperator>()
            .FirstOrDefault(it => it.Operand.Equals(type));

        return bestMatched;
    }

    public IndexOperator? GetBestMatchedIndexOperator(LuaType type, LuaType key)
    {
        if (!TypeOperators.TryGetValue(TypeOperatorKind.Index, out var entry))
        {
            return null;
        }

        var bestMatched = entry.Files.Values
            .SelectMany(it => it)
            .OfType<IndexOperator>()
            .FirstOrDefault(it => it.Type.Equals(type) && it.Key.Equals(key));
        return bestMatched;
    }
}
