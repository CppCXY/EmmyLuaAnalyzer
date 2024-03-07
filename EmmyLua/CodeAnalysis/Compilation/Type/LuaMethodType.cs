using EmmyLua.CodeAnalysis.Compilation.Infer;
using EmmyLua.CodeAnalysis.Compilation.Symbol;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace EmmyLua.CodeAnalysis.Compilation.Type;

public class LuaMultiReturnType(List<LuaType> retTypes) : LuaType(TypeKind.Return)
{
    public List<LuaType> RetTypes { get; } = retTypes;
}

public class LuaSignature(LuaType returnType, List<ParameterDeclaration> parameters)
{
    public LuaType ReturnType { get; set; } = returnType;

    public List<ParameterDeclaration> Parameters { get; } = parameters;


}

public class LuaMethodType(LuaSignature mainSignature, List<LuaSignature>? overloads, bool colon)
    : LuaType(TypeKind.Method)
{
    public LuaSignature MainSignature { get; } = mainSignature;

    public List<LuaSignature>? Overloads { get; } = overloads;

    public LuaType? SelfType { get; set; } = null;

    public bool Colon { get; } = colon;

    public LuaMethodType(LuaType returnType, List<ParameterDeclaration> parameters, bool colon)
        : this(new LuaSignature(returnType, parameters), null, colon)
    {
    }

    public LuaSignature FindPerfectMatchSignature(
        LuaCallExprSyntax callExpr,
        List<LuaExprSyntax> args,
        SearchContext context)
    {
        if (Overloads is null)
        {
            return MainSignature;
        }


        throw new NotImplementedException();
    }
}
