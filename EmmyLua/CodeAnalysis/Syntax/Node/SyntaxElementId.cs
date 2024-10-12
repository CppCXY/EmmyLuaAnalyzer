using EmmyLua.CodeAnalysis.Compilation;
using EmmyLua.CodeAnalysis.Compilation.Search;
using EmmyLua.CodeAnalysis.Document;

namespace EmmyLua.CodeAnalysis.Syntax.Node;

public readonly record struct SyntaxElementId(LuaDocumentId DocumentId, int ElementId)
{
    public static readonly SyntaxElementId Empty = new SyntaxElementId(new LuaDocumentId(0), 0);

    public static SyntaxElementId From(string idString)
    {
        var longId = long.Parse(idString);
        return new SyntaxElementId(new LuaDocumentId((int)(longId >> 32)), (int)longId);
    }

    public long UniqueId => ((long)DocumentId.Id << 32) | (uint)ElementId;

    public string Stringify => UniqueId.ToString();

    public LuaLocation GetLocation(SearchContext context)
    {
        var document = context.Compilation.Project.GetDocument(DocumentId);
        if (document is not null)
        {
            var element = document.SyntaxTree.GetElement(ElementId);
            if (element is not null)
            {
                return element.Location;
            }
        }

        return LuaLocation.Empty;
    }

    public LuaSyntaxElement? ToElement(LuaDocument document)
    {
        return document.SyntaxTree.GetElement(ElementId);
    }

    public override string ToString()
    {
        return Stringify;
    }
}
