using EmmyLua.CodeAnalysis.Compilation.Infer;
using EmmyLua.CodeAnalysis.Syntax.Node;

namespace EmmyLua.CodeAnalysis.Compilation.Type;

public class LuaVarRef(LuaSyntaxElement element, LuaExprRef? exprRef)
    : LuaClass($"{element.Tree.Document.Id.Guid}|{element.Range.StartOffset}")
{
    public LuaExprRef? ExprRef { get; } = exprRef;

    public override bool SubTypeOf(ILuaType other, SearchContext context)
    {
        if (ExprRef is not null)
        {
            return ExprRef.GetType(context).SubTypeOf(other, context);
        }

        return other.IsNullable;
    }
}
