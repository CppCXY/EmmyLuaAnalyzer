using EmmyLua.CodeAnalysis.Syntax.Node;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace EmmyLua.CodeAnalysis.Compilation.Type;

public readonly struct LuaTypeId(SyntaxElementId id)
{
    public static LuaTypeId Create(LuaDocTypeSyntax docType)
    {
        return new(docType.UniqueId);
    }

    public SyntaxElementId Id { get; } = id;
}
