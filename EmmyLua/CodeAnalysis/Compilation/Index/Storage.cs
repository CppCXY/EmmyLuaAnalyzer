using EmmyLua.CodeAnalysis.Document;

namespace EmmyLua.CodeAnalysis.Compilation.Index;

public class IndexEntry<TStubElement>
{
    public Dictionary<DocumentId, List<TStubElement>> Files { get; } = new();

    public void Add(DocumentId documentId, TStubElement element)
    {
        if (!Files.TryGetValue(documentId, out var elements))
        {
            elements = new List<TStubElement>();
            Files.Add(documentId, elements);
        }

        elements.Add(element);
    }

    public void Remove(DocumentId documentId)
    {
        Files.Remove(documentId);
    }
}

public class IndexStorage<TKey, TStubElement>
    where TKey : notnull
{
    private readonly Dictionary<TKey, IndexEntry<TStubElement>> _indexMap = new();

    public void Add(DocumentId documentId, TKey key, TStubElement syntax)
    {
        if (!_indexMap.TryGetValue(key, out var entry))
        {
            entry = new IndexEntry<TStubElement>();
            _indexMap.Add(key, entry);
        }

        entry.Add(documentId, syntax);
    }

    public void Remove(DocumentId documentId)
    {
        var waitRemove = new List<TKey>();
        foreach (var (key, entry) in _indexMap)
        {
            entry.Remove(documentId);
            if (entry.Files.Count == 0)
            {
                waitRemove.Add(key);
            }
        }

        foreach (var key in waitRemove)
        {
            _indexMap.Remove(key);
        }
    }

    public IEnumerable<TStubElement> Get(TKey key)
    {
        return _indexMap.TryGetValue(key, out var entry)
            ? entry.Files.Values.SelectMany(it => it)
            : Enumerable.Empty<TStubElement>();
    }

    public TStubElement? GetOne(TKey key)
    {
        return Get(key).FirstOrDefault();
    }

    public IEnumerable<TValue> Get<TValue>(TKey key)
        where TValue : TStubElement
        => Get(key).OfType<TValue>();

    public IEnumerable<TStubElement> GetAll() =>
        _indexMap.Values.SelectMany(
            it => it.Files.Values.SelectMany(it2 => it2));
}

public class SyntaxStorage<TKey, TSyntaxElement, TElement>
    where TKey : notnull
{
    private readonly Dictionary<TKey, IndexEntry<(TSyntaxElement, TElement)>> _storage = new();

    public void Add(DocumentId documentId, TKey key, TSyntaxElement syntax, TElement element)
    {
        if (!_storage.TryGetValue(key, out var entry))
        {
            entry = new IndexEntry<(TSyntaxElement, TElement)>();
            _storage.Add(key, entry);
        }

        entry.Add(documentId, (syntax, element));
    }

    public void Remove(DocumentId documentId)
    {
        var waitRemove = new List<TKey>();
        foreach (var (key, entry) in _storage)
        {
            entry.Remove(documentId);
            if (entry.Files.Count == 0)
            {
                waitRemove.Add(key);
            }
        }

        foreach (var key in waitRemove)
        {
            _storage.Remove(key);
        }
    }

    public IEnumerable<(TSyntaxElement, TElement)> Get(TKey key)
    {
        return _storage.TryGetValue(key, out var entry)
            ? entry.Files.Values.SelectMany(it => it)
            : Enumerable.Empty<(TSyntaxElement, TElement)>();
    }
}
