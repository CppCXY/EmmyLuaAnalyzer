using EmmyLua.CodeAnalysis.Compilation.Type;
using EmmyLua.CodeAnalysis.Document;

namespace EmmyLua.CodeAnalysis.Compilation.Index;

public class TypeOperatorStorage
{
    private Dictionary<string, IndexEntry<TypeOperator>> TypeOperators { get; } = new();

    public void AddTypeOperator(LuaDocumentId documentId, TypeOperator typeOperator)
    {
        var belongTypeName = typeOperator.BelongTypeName;
        if (!TypeOperators.TryGetValue(typeOperator.BelongTypeName, out var entry))
        {
            entry = new IndexEntry<TypeOperator>();
            TypeOperators.Add(belongTypeName, entry);
        }

        entry.Add(documentId, typeOperator);
    }

    public void Remove(LuaDocumentId documentId)
    {
        RemoveOperator(documentId);
    }

    private void RemoveOperator(LuaDocumentId documentId)
    {
        var waitRemove = new List<string>();
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

    public IEnumerable<TypeOperator> GetTypeOperators(string typeName)
    {
        if (TypeOperators.TryGetValue(typeName, out var entry))
        {
            return entry.Files.Values.SelectMany(it => it);
        }

        return Enumerable.Empty<TypeOperator>();
    }
}
