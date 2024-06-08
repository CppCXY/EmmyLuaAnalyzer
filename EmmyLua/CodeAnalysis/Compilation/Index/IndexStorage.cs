using EmmyLua.CodeAnalysis.Document;
using EmmyLua.CodeAnalysis.Type;

namespace EmmyLua.CodeAnalysis.Compilation.Index;

public class IndexStorage<TKey, TStubElement>
    where TKey : notnull
{
    record struct ElementIndex(LuaDocumentId DocumentId, TStubElement Element);

    private readonly Dictionary<TKey, List<ElementIndex>> _indexMap = new();

    public void Add(LuaDocumentId documentId, TKey key, TStubElement element)
    {
        if (!_indexMap.TryGetValue(key, out var elements))
        {
            elements = new();
            _indexMap.Add(key, elements);
        }

        elements.Add(new ElementIndex(documentId, element));
    }

    public void Remove(LuaDocumentId documentId)
    {
        var waitRemove = new List<TKey>();
        foreach (var (key, elements) in _indexMap)
        {
            elements.RemoveAll(it => it.DocumentId == documentId);
            if (elements.Count == 0)
            {
                waitRemove.Add(key);
            }
        }

        foreach (var key in waitRemove)
        {
            _indexMap.Remove(key);
        }
    }

    public IEnumerable<TStubElement> Query(TKey key)
    {
        if (_indexMap.TryGetValue(key, out var elements))
        {
            return elements.Select(it => it.Element);
        }

        return [];
    }

    public IEnumerable<TStubElement> QueryAll() =>
        _indexMap.Values.SelectMany(it => it.Select(element => element.Element));

    public bool ContainsKey(TKey key) => _indexMap.ContainsKey(key);
}
