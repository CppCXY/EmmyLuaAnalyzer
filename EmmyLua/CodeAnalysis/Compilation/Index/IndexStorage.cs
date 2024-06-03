using EmmyLua.CodeAnalysis.Document;
using EmmyLua.CodeAnalysis.Type;

namespace EmmyLua.CodeAnalysis.Compilation.Index;

public class IndexEntry<TStubElement>
{
    private LuaDocumentId _hotDocumentId = LuaDocumentId.VirtualDocumentId;

    private List<TStubElement>? _hotElements;

    public Dictionary<LuaDocumentId, List<TStubElement>> Files { get; } = new();

    public void Add(LuaDocumentId documentId, TStubElement element)
    {
        if (documentId == _hotDocumentId && _hotElements is not null)
        {
            _hotElements.Add(element);
            return;
        }

        if (!Files.TryGetValue(documentId, out var elements))
        {
            elements = [];
            Files.Add(documentId, elements);
        }

        _hotDocumentId = documentId;
        _hotElements = elements;

        elements.Add(element);
    }

    public void Remove(LuaDocumentId documentId)
    {
        Files.Remove(documentId);
        if (_hotDocumentId == documentId)
        {
            _hotDocumentId = LuaDocumentId.VirtualDocumentId;
            _hotElements = null;
        }
    }
}

public class IndexStorage<TKey, TStubElement>
    where TKey : notnull
{
    private readonly Dictionary<TKey, IndexEntry<TStubElement>> _indexMap = new();

    public void Add(LuaDocumentId documentId, TKey key, TStubElement element)
    {
        if (!_indexMap.TryGetValue(key, out var entry))
        {
            entry = new IndexEntry<TStubElement>();
            _indexMap.Add(key, entry);
        }

        entry.Add(documentId, element);
    }

    public void Remove(LuaDocumentId documentId)
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

    public IEnumerable<TStubElement> Query(TKey key)
    {
        return _indexMap.TryGetValue(key, out var entry)
            ? entry.Files.Values.SelectMany(it => it)
            : [];
    }

    public IEnumerable<TStubElement> QueryAll() =>
        _indexMap.Values.SelectMany(
            it => it.Files.Values.SelectMany(it2 => it2));

    public bool ContainsKey(TKey key) => _indexMap.ContainsKey(key);
}

public class SyntaxStorage<TKey, TSyntaxElement, TElement>
    where TKey : notnull
{
    private readonly Dictionary<TKey, IndexEntry<(TSyntaxElement, TElement)>> _storage = new();

    public void Add(LuaDocumentId documentId, TKey key, TSyntaxElement syntax, TElement element)
    {
        if (!_storage.TryGetValue(key, out var entry))
        {
            entry = new IndexEntry<(TSyntaxElement, TElement)>();
            _storage.Add(key, entry);
        }

        entry.Add(documentId, (syntax, element));
    }

    public void Remove(LuaDocumentId documentId)
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
            : [];
    }
}

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
        return TypeOperators.TryGetValue(typeName, out var entry) ? entry.Files.Values.SelectMany(it => it) : [];
    }
}
