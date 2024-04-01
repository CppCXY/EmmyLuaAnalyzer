using EmmyLua.CodeAnalysis.Syntax.Tree;
using System;

namespace EmmyLua.CodeAnalysis.Document;

public readonly struct LuaDocumentId(int id)
{
    public static LuaDocumentId VirtualDocumentId { get; } = new(0);

    public int Id { get; } = id;

    public bool IsVirtual => Id == 0;

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }

    public static bool operator ==(LuaDocumentId left, LuaDocumentId right)
    {
        return left.Id == right.Id;
    }

    public static bool operator !=(LuaDocumentId left, LuaDocumentId right)
    {
        return !(left == right);
    }

    public override string ToString()
    {
        return Id.ToString();
    }
}

public sealed class LuaDocument
{
    public LuaDocumentId Id { get; set; }

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
        return new LuaDocument(fileText, language, LuaDocumentId.VirtualDocumentId, uri.AbsoluteUri, uri.LocalPath);
    }

    public static LuaDocument FromText(string text, LuaLanguage language)
    {
        return new LuaDocument(text, language, LuaDocumentId.VirtualDocumentId, string.Empty, string.Empty);
    }

    public static LuaDocument FromUri(string uri, string text, LuaLanguage language)
    {
        var uri2 = new Uri(uri);
        return new LuaDocument(text, language, LuaDocumentId.VirtualDocumentId, uri2.AbsoluteUri, uri2.LocalPath);
    }

    public static LuaDocument FromPath(string path, string text, LuaLanguage language)
    {
        var uri = new Uri(path);
        return new LuaDocument(text, language, LuaDocumentId.VirtualDocumentId, uri.AbsoluteUri, uri.LocalPath);
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
