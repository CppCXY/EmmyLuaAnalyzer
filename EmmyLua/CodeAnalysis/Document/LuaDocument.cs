using EmmyLua.CodeAnalysis.Syntax.Tree;
using System;

namespace EmmyLua.CodeAnalysis.Document;

public readonly struct DocumentId(int id) : IEquatable<DocumentId>
{
    public static DocumentId VirtualDocumentId { get; } = new(0);

    public int Id { get; } = id;

    public bool IsVirtual => Id == 0;

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }

    public override bool Equals(object? obj)
    {
        return obj is DocumentId other && Equals(other);
    }

    public bool Equals(DocumentId other)
    {
        return other.Id == Id;
    }

    public static bool operator ==(DocumentId left, DocumentId right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(DocumentId left, DocumentId right)
    {
        return !(left == right);
    }
}

public class LuaDocument
{
    public DocumentId Id { get; set; }

    public string Uri { get; }

    public string Path { get; }

    public bool IsVirtual => Uri.Length == 0;

    private LuaSyntaxTree? _syntaxTree;

    public string Text { get; }

    public LuaLanguage Language { get; }

    private LineIndex LineIndex { get; }

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
        return new LuaDocument(fileText, language, DocumentId.VirtualDocumentId, uri.AbsoluteUri, uri.LocalPath);
    }

    public static LuaDocument FromText(string text, LuaLanguage language)
    {
        return new LuaDocument(text, language, DocumentId.VirtualDocumentId, string.Empty, string.Empty);
    }

    public static LuaDocument FromUri(string uri, string text, LuaLanguage language)
    {
        var uri2 = new Uri(uri);
        return new LuaDocument(text, language, DocumentId.VirtualDocumentId, uri2.AbsoluteUri, uri2.LocalPath);
    }

    public static LuaDocument FromPath(string path, string text, LuaLanguage language)
    {
        var uri = new Uri(path);
        return new LuaDocument(text, language, DocumentId.VirtualDocumentId, uri.AbsoluteUri, uri.LocalPath);
    }

    private LuaDocument(string text, LuaLanguage language, DocumentId id, string uri, string path)
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

    public LuaDocument WithText(string text)
    {
        return new LuaDocument(text, Language, Id, Uri, Path);
    }
}
