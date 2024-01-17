using EmmyLua.CodeAnalysis.Compile;
using EmmyLua.CodeAnalysis.Compile.Source;
using EmmyLua.CodeAnalysis.Syntax.Tree;

namespace EmmyLua.CodeAnalysis.Workspace;

public class DocumentId(string path, string uri)
{
    public static DocumentId FromUri(string url)
    {
        var uri = new Uri(url);
        return new DocumentId(uri.AbsolutePath, url);
    }

    public static DocumentId FromPath(string path)
    {
        return new DocumentId(path, new Uri(path).AbsoluteUri);
    }

    public static DocumentId VirtualDocumentId()
    {
        return new DocumentId(string.Empty, string.Empty);
    }

    public string Path { get; } = path;

    public string Url { get; } = uri;

    public string Guid { get; } = System.Guid.NewGuid().ToString();

    public bool IsVirtual => Path.Length == 0;
}

public class LuaDocument : LuaSource
{
    public DocumentId Id { get; }

    private LuaSyntaxTree? _syntaxTree;

    public static LuaDocument OpenDocument(string path, LuaLanguage language)
    {
        var fileText = File.ReadAllText(path);
        var documentId = DocumentId.FromPath(path);
        return new LuaDocument(fileText, language, documentId);
    }

    public static LuaDocument FromPath(string path, string text, LuaLanguage language)
    {
        var documentId = DocumentId.FromPath(path);
        return new LuaDocument(text, language, documentId);
    }

    public static LuaDocument FromUri(string uri, string text, LuaLanguage language)
    {
        var documentId = DocumentId.FromUri(uri);
        return new LuaDocument(text, language, documentId);
    }

    public static LuaDocument FromText(string text, LuaLanguage language)
    {
        var documentId = DocumentId.VirtualDocumentId();
        return new LuaDocument(text, language, documentId);
    }

    private LuaDocument(string text, LuaLanguage language, DocumentId id)
        : base(text, language)
    {
        Id = id;
    }

    public LuaSyntaxTree SyntaxTree => _syntaxTree ??= LuaSyntaxTree.Create(this);

    public override LuaDocumentLocation GetLocation(SourceRange range)
    {
        return new LuaDocumentLocation(this, range);
    }

    public LuaDocument WithText(string text)
    {
        return new LuaDocument(text, Language, Id);
    }
}
