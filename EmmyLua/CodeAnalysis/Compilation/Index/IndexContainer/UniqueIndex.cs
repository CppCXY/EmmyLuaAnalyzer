using EmmyLua.CodeAnalysis.Document;

namespace EmmyLua.CodeAnalysis.Compilation.Index.IndexContainer;

public class UniqueIndex<TKey, TStubElement> where TKey : notnull
{
    record struct ElementIndex(LuaDocumentId DocumentId, TStubElement Element);

    private readonly Dictionary<TKey, ElementIndex> _indexMap = new();

    public void Update(LuaDocumentId documentId, TKey key, TStubElement element)
    {
        _indexMap[key] = new ElementIndex(documentId, element);
    }

    public void Remove(LuaDocumentId documentId)
    {
        var waitRemove = new List<TKey>();
        foreach (var (key, element) in _indexMap)
        {
            if (element.DocumentId == documentId)
            {
                waitRemove.Add(key);
            }
        }

        foreach (var key in waitRemove)
        {
            _indexMap.Remove(key);
        }
    }

    public TStubElement? Query(TKey key)
    {
        if (_indexMap.TryGetValue(key, out var element))
        {
            return element.Element;
        }

        return default;
    }

    public IEnumerable<TStubElement> QueryAll() =>
        _indexMap.Values.Select(it => it.Element);

    public bool ContainsKey(TKey key) => _indexMap.ContainsKey(key);
}
