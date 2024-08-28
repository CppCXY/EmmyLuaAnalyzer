using EmmyLua.CodeAnalysis.Syntax.Node;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace EmmyLua.CodeAnalysis.Compilation.Declaration;

public readonly struct TypeId(SyntaxElementId Id)
{
    public static TypeId Create(LuaDocTypeSyntax docType)
    {
        return new(docType.UniqueId);
    }
}
