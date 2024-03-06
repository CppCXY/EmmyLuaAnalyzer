using EmmyLua.CodeAnalysis.Compilation.Symbol;

namespace EmmyLua.CodeAnalysis.Compilation.Type;

public class LuaReturnType(List<LuaType> retTypes) : LuaType(TypeKind.Return)
{
    public List<LuaType> RetTypes { get; } = retTypes;
}

public class LuaSignature(LuaReturnType returnType, List<ParameterDeclaration> parameters)
{
    public LuaReturnType ReturnType { get; set; } = returnType;

    public List<ParameterDeclaration> Parameters { get; } = parameters;
}

public class LuaMethodType(LuaSignature mainSignature, List<LuaSignature>? overloads, bool colon) : LuaType(TypeKind.Method)
{
    public LuaSignature MainSignature { get; } = mainSignature;

    public List<LuaSignature>? Overloads { get; } = overloads;

    public LuaType? SelfType { get; set; } = null;

    public bool Colon { get; } = colon;

    public LuaMethodType(LuaReturnType returnType, List<ParameterDeclaration> parameters, bool colon)
        : this(new LuaSignature(returnType, parameters), null, colon)
    {
    }
}
