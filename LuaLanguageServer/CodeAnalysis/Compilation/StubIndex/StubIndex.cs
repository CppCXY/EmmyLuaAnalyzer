using LuaLanguageServer.CodeAnalysis.Syntax.Node;
using LuaLanguageServer.CodeAnalysis.Workspace;

namespace LuaLanguageServer.CodeAnalysis.Compilation.StubIndex;

public class StubIndex<TKey, TSyntax>
    where TKey : notnull
    where TSyntax : LuaSyntaxNode
{
    private class StubFile
    {
        public List<TSyntax> Elements { get; set; } = new();
    }

    private class StubEntry
    {
        public TKey Key { get; set; }
        public Dictionary<DocumentId, StubFile> Files { get; set; } = new();
    }

    private readonly Dictionary<TKey, StubEntry> _indexMap = new();

    public void AddStub(DocumentId documentId, TKey key, TSyntax syntax)
    {
        if (!_indexMap.TryGetValue(key, out var entry))
        {
            entry = new StubEntry()
            {
                Key = key
            };
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

    public IEnumerable<TSyntax> Get(TKey key)
    {
        return _indexMap.TryGetValue(key, out var entry)
            ? entry.Files.Values.SelectMany(it => it.Elements)
            : Enumerable.Empty<TSyntax>();
    }
}
