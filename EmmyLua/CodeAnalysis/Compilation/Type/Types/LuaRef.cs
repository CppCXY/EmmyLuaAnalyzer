using EmmyLua.CodeAnalysis.Compilation.Declaration;
using EmmyLua.CodeAnalysis.Compilation.Search;
using EmmyLua.CodeAnalysis.Document;
using EmmyLua.CodeAnalysis.Syntax.Node;

namespace EmmyLua.CodeAnalysis.Compilation.Type.Types;

public class LuaTypeRef(LuaTypeId id) : LuaType
{
    public LuaTypeId Id { get; } = id;

    public LuaDocumentId DocumentId => Id.Id.DocumentId;
}

public class LuaElementRef(SyntaxElementId id)
    : LuaType
{
    public SyntaxElementId Id { get; } = id;

    public LuaSyntaxElement? ToSyntaxElement(SearchContext context)
    {
        var document = context.Compilation.Project.GetDocument(Id.DocumentId);
        return document?.SyntaxTree.GetElement(Id.ElementId);
    }
}
