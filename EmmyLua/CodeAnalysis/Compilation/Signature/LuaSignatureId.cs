using EmmyLua.CodeAnalysis.Syntax.Node;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace EmmyLua.CodeAnalysis.Compilation.Signature;

public readonly record struct LuaSignatureId(SyntaxElementId Id)
{
    public static LuaSignatureId Create(LuaClosureExprSyntax closureExpr)
    {
        return new LuaSignatureId(closureExpr.UniqueId);
    }

    public static LuaSignatureId Create(LuaDocFuncTypeSyntax docFuncType)
    {
        return new LuaSignatureId(docFuncType.UniqueId);
    }

    public LuaElementPtr<LuaClosureExprSyntax> ClosurePtr => new(Id);

    public LuaElementPtr<LuaDocFuncTypeSyntax> DocFuncTypePtr => new(Id);
}
