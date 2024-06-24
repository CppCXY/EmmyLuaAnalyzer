using EmmyLua.CodeAnalysis.Document;

namespace EmmyLua.CodeAnalysis.Compilation.Index.IndexContainer;

public class InFileIndex<TKey, TValue> where TKey : notnull
{
    private readonly Dictionary<LuaDocumentId, Dictionary<TKey, TValue>> _map = new();

    public void Add(LuaDocumentId documentId, TKey key, TValue value)
    {
        if (!_map.TryGetValue(documentId, out var documentMap))
        {
            documentMap = new();
            _map.Add(documentId, documentMap);
        }

        documentMap[key] = value;
    }

    public void Remove(LuaDocumentId documentId)
    {
        _map.Remove(documentId);
    }

    public TValue? Query(LuaDocumentId documentId, TKey key)
    {
        if (_map.TryGetValue(documentId, out var documentMap))
        {
            if (documentMap.TryGetValue(key, out var value))
            {
                return value;
            }
        }

        return default;
    }

    public IEnumerable<TValue> QueryAll(LuaDocumentId documentId)
    {
        if (_map.TryGetValue(documentId, out var documentMap))
        {
            return documentMap.Values;
        }

        return [];
    }
}
