using EmmyLua.CodeAnalysis.Document;

namespace EmmyLua.CodeAnalysis.Container;

public class InFileIndex<TKey, TValue> where TKey : notnull
{
    private readonly Dictionary<TKey, TValue> _map = new();

    private readonly Dictionary<LuaDocumentId, HashSet<TKey>> _documentKeys = new();

    public void Add(LuaDocumentId documentId, TKey key, TValue value)
    {
        _map[key] = value;

        if (!_documentKeys.TryGetValue(documentId, out var keys))
        {
            keys = new HashSet<TKey>();
            _documentKeys[documentId] = keys;
        }

        keys.Add(key);
    }

    public void Remove(LuaDocumentId documentId)
    {
        if (_documentKeys.TryGetValue(documentId, out var keys))
        {
            foreach (var key in keys)
            {
                _map.Remove(key);
            }

            _documentKeys.Remove(documentId);
        }
    }

    public TValue? Query(TKey key)
    {
        return _map.GetValueOrDefault(key);
    }

    public bool ContainsKey(TKey key)
    {
        return _map.ContainsKey(key);
    }

    public IEnumerable<TValue> QueryAll(LuaDocumentId documentId)
    {
        if (_documentKeys.TryGetValue(documentId, out var keys))
        {
            foreach (var key in keys)
            {
                if (_map.TryGetValue(key, out var value))
                {
                    yield return value;
                }
            }
        }
    }
}
