using EmmyLua.CodeAnalysis.Common;
using EmmyLua.CodeAnalysis.Syntax.Tree;

namespace EmmyLua.CodeAnalysis.Document;

public readonly record struct LuaDocumentId(int Id)
{
    public static LuaDocumentId VirtualDocumentId { get; } = new(0);

    public bool IsVirtual => Id == 0;
}

public sealed class LuaDocument : IDocument
{
    public static readonly LuaDocument Empty = new(string.Empty, LuaLanguage.Default, LuaDocumentId.VirtualDocumentId,
        string.Empty, string.Empty);

    public LuaDocumentId Id { get; set; }

    public OpenState OpenState { get; set; } = OpenState.Closed;

    public string Uri { get; }

    public string Path { get; }

    public bool IsVirtual => Uri.Length == 0;

    private LuaSyntaxTree? _syntaxTree;

    public string Text { get; private set; }

    public LuaLanguage Language { get; }

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
        var uri = new Uri(path);
        return new LuaDocument(fileText, language, LuaDocumentId.VirtualDocumentId,
            System.Uri.UnescapeDataString(uri.AbsoluteUri), path);
    }

    public static LuaDocument FromText(string text, LuaLanguage language)
    {
        return new LuaDocument(text, language, LuaDocumentId.VirtualDocumentId, string.Empty, string.Empty);
    }

    public static LuaDocument FromUri(string uri, string text, LuaLanguage language)
    {
        var uri2 = new Uri(uri);
        return new LuaDocument(text, language, LuaDocumentId.VirtualDocumentId,
            System.Uri.UnescapeDataString(uri), uri2.LocalPath);
    }

    public static LuaDocument FromPath(string path, string text, LuaLanguage language)
    {
        var uri = new Uri(path);
        return new LuaDocument(text, language, LuaDocumentId.VirtualDocumentId,
            System.Uri.UnescapeDataString(uri.AbsoluteUri), path);
    }

    private LuaDocument(string text, LuaLanguage language, LuaDocumentId id, string uri, string path)
    {
        Id = id;
        Text = text;
        Language = language;
        LineIndex = LineIndex.Parse(text);
        Uri = uri;
        Path = path;
    }

    public LuaSyntaxTree SyntaxTree => _syntaxTree ??= LuaSyntaxTree.Create(this);

    public LuaLocation GetLocation(SourceRange range, int baseLine = 0)
    {
        return new LuaLocation(this, range, baseLine);
    }

    public void ReplaceText(string text)
    {
        _syntaxTree = null;
        LineIndex = LineIndex.Parse(text);
        Text = text;
    }
}
