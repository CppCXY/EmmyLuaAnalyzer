using EmmyLua.CodeAnalysis.Document;

namespace EmmyLua.CodeAnalysis.Compilation.Index.IndexContainer;

public class PriorityIndex<TKey, TStubElement> where TKey : notnull
{
    record struct ElementIndex(LuaDocumentId DocumentId, TStubElement Element);

    private Dictionary<TKey, List<ElementIndex>> _indexMap = new();

    public void AddGlobal(LuaDocumentId documentId, TKey key, TStubElement element, bool highestPriority = false)
    {
        if (!_indexMap.TryGetValue(key, out var elementIndices))
        {
            elementIndices = new();
            _indexMap.Add(key, elementIndices);
        }

        if (highestPriority && elementIndices.Count != 0)
        {
            elementIndices.Insert(0, new ElementIndex(documentId, element));
        }
        else
        {
            elementIndices.Add(new ElementIndex(documentId, element));
        }
    }

    public TStubElement? Query(TKey key)
    {
        if (_indexMap.TryGetValue(key, out var elements))
        {
            return elements.First().Element;
        }

        return default;
    }

    public IEnumerable<TStubElement> QueryAll()
    {
        return _indexMap.Values.Select(it => it.First().Element);
    }

    public void Remove(LuaDocumentId documentId)
    {
        var waitRemove = new List<TKey>();
        foreach (var (key, declarations) in _indexMap)
        {
            declarations.RemoveAll(it => it.DocumentId == documentId);
            if (declarations.Count == 0)
            {
                waitRemove.Add(key);
            }
        }

        foreach (var key in waitRemove)
        {
            _indexMap.Remove(key);
        }
    }
}
