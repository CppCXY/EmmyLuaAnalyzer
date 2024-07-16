using EmmyLua.CodeAnalysis.Document;
using EmmyLua.CodeAnalysis.Syntax.Node;
using EmmyLua.LanguageServer.Framework.Protocol.Message.SemanticToken;
using EmmyLua.LanguageServer.Framework.Protocol.Model;

namespace EmmyLua.LanguageServer.SemanticToken;

public class SemanticBuilderWrapper(SemanticTokensBuilder builder, LuaDocument document, bool multiLineSupport)
{
    public void Push(LuaSyntaxElement element, string type)
    {
        var range = element.Range;
        var startLine = document.GetLine(range.StartOffset);
        var startCol = document.GetCol(range.StartOffset);
        var endLine = document.GetLine(range.EndOffset);
        if (!multiLineSupport && startLine != endLine)
        {
            builder.Push(new Position(startLine, startCol), 9999, type);
            for (var i = startLine + 1; i < endLine - 1; i++)
            {
                builder.Push(new Position(i, 0), 9999, type);
            }

            builder.Push(new Position(endLine, 0), document.GetCol(range.EndOffset), type);

            return;
        }

        builder.Push(new Position(startLine, startCol), range.Length, type);
    }

    public void Push(LuaSyntaxElement element, string type, string modifier)
    {
        var range = element.Range;
        var startLine = document.GetLine(range.StartOffset);
        var startCol = document.GetCol(range.StartOffset);
        var endLine = document.GetLine(range.EndOffset);
        if (!multiLineSupport && startLine != endLine)
        {
            builder.Push(new Position(startLine, startCol), 9999, type, modifier);
            for (var i = startLine + 1; i < endLine - 1; i++)
            {
                builder.Push(new Position(i, 0), 9999, type, modifier);
            }

            builder.Push(new Position(endLine, 0), document.GetCol(range.EndOffset), type, modifier);

            return;
        }

        builder.Push(new Position(startLine, startCol), range.Length, type, modifier);
    }

    public void Push(LuaSyntaxElement element, string type, List<string> modifiers)
    {
        var range = element.Range;
        var startLine = document.GetLine(range.StartOffset);
        var startCol = document.GetCol(range.StartOffset);
        var endLine = document.GetLine(range.EndOffset);
        if (!multiLineSupport && startLine != endLine)
        {
            builder.Push(new Position(startLine, startCol), 9999, type, modifiers);
            for (var i = startLine + 1; i < endLine - 1; i++)
            {
                builder.Push(new Position(i, 0), 9999, type, modifiers);
            }

            builder.Push(new Position(endLine, 0), document.GetCol(range.EndOffset), type, modifiers);

            return;
        }

        builder.Push(new Position(startLine, startCol), range.Length, type, modifiers);
    }

    public List<uint> Build()
    {
        return builder.Build();
    }
}