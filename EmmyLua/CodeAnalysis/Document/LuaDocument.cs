using EmmyLua.CodeAnalysis.Syntax.Tree;

namespace EmmyLua.CodeAnalysis.Document;

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

public class LuaDocument
{
    public DocumentId Id { get; }

    private LuaSyntaxTree? _syntaxTree;

    public string Text { get; set; }

    public LuaLanguage Language { get; set; }

    private LineIndex LineIndex { get; set; }

    public int GetCol(int offset)
    {
        return LineIndex.GetCol(offset, Text);
    }

    public int GetLine(int offset)
    {
        return LineIndex.GetLine(offset);
    }

    public int GetOffset(int line, int col)
    {
        return LineIndex.GetOffset(line, col, Text);
    }

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
    {
        Id = id;
        Text = text;
        Language = language;
        LineIndex = LineIndex.Parse(text);
    }

    public LuaSyntaxTree SyntaxTree => _syntaxTree ??= LuaSyntaxTree.Create(this);

    public LuaLocation GetLocation(SourceRange range, int baseLine = 0)
    {
        return new LuaLocation(this, range, baseLine);
    }

    public LuaDocument WithText(string text)
    {
        return new LuaDocument(text, Language, Id);
    }
}

