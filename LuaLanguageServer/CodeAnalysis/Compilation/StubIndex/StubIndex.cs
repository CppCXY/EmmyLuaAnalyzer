using LuaLanguageServer.CodeAnalysis.Syntax.Node;
using LuaLanguageServer.CodeAnalysis.Workspace;

namespace LuaLanguageServer.CodeAnalysis.Compilation.StubIndex;

public class StubIndex<Key, Syntax>
    where Key : notnull
    where Syntax : LuaSyntaxNode
{
    class StubFile
    {
        public List<Syntax> Elements { get; set; }
    }

    class StubEntry
    {
        public Key Key { get; set; }
        public Dictionary<DocumentId, StubFile> Files { get; set; } = new();
    }

    private readonly Dictionary<Key, StubEntry> _indexMap = new();

    public void AddStub(DocumentId documentId, Key key, Syntax syntax)
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
            file = new StubFile()
            {
                Elements = new List<Syntax>()
            };
            entry.Files.Add(documentId, file);
        }

        file.Elements.Add(syntax);
    }

    public void RemoveStub(DocumentId documentId)
    {
        var waitRemove = new List<Key>();
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

    public IEnumerable<Syntax> Get(Key key)
    {
        return _indexMap.TryGetValue(key, out var entry)
            ? entry.Files.Values.SelectMany(it => it.Elements)
            : Enumerable.Empty<Syntax>();
    }
}
