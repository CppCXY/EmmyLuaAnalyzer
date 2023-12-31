﻿using EmmyLua.CodeAnalysis.Workspace;

namespace EmmyLua.CodeAnalysis.Compilation.Analyzer.Stub;

public class StubIndex<TKey, TStubElement>
    where TKey : notnull
{
    private class StubFile
    {
        public List<TStubElement> Elements { get; set; } = [];
    }

    private class StubEntry(TKey key)
    {
        public TKey Key { get; set; } = key;
        public Dictionary<DocumentId, StubFile> Files { get; set; } = new();
    }

    private readonly Dictionary<TKey, StubEntry> _indexMap = new();

    public void AddStub(DocumentId documentId, TKey key, TStubElement syntax)
    {
        if (!_indexMap.TryGetValue(key, out var entry))
        {
            entry = new StubEntry(key);
            _indexMap.Add(key, entry);
        }

        if (!entry.Files.TryGetValue(documentId, out var file))
        {
            file = new StubFile();
            entry.Files.Add(documentId, file);
        }

        file.Elements.Add(syntax);
    }

    public void RemoveStub(DocumentId documentId)
    {
        var waitRemove = new List<TKey>();
        foreach (var (key, entry) in _indexMap)
        {
            entry.Files.Remove(documentId);
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

    private IEnumerable<TStubElement> Get(TKey key)
    {
        return _indexMap.TryGetValue(key, out var entry)
            ? entry.Files.Values.SelectMany(it => it.Elements)
            : Enumerable.Empty<TStubElement>();
    }

    public IEnumerable<TValue> Get<TValue>(TKey key)
        where TValue : TStubElement
        => Get(key).OfType<TValue>();
}
