using EmmyLuaAnalyzer.CodeAnalysis.Compile;
using EmmyLuaAnalyzer.CodeAnalysis.Compile.Source;
using EmmyLuaAnalyzer.CodeAnalysis.Syntax.Tree;

namespace EmmyLuaAnalyzer.CodeAnalysis.Workspace;

public class DocumentId(string path)
{
    public string Path { get; } = path;

    public string Url { get; } = new Uri(path).AbsoluteUri;

    public string Guid { get; } = System.Guid.NewGuid().ToString();
}

public class LuaDocument : LuaSource
{
    public DocumentId Id { get; }

    private LuaSyntaxTree? _syntaxTree;

    public static LuaDocument OpenDocument(string path, LuaLanguage language)
    {
        var fileText = File.ReadAllText(path);
        var documentId = new DocumentId(path);
        return new LuaDocument(fileText, language, documentId);
    }

    public static LuaDocument From(string path, string text, LuaLanguage language)
    {
        var documentId = new DocumentId(path);
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
}
