namespace EmmyLua.CodeAnalysis.Compilation.Type;

public class LuaSignature
{

}

public class TypedParameter(string name, LuaType? type)
{
    public string Name { get; } = name;
    public LuaType? Type { get; } = type;
}

public class LuaReturnType(List<LuaType> retTypes) : LuaType(TypeKind.Return)
{
    public List<LuaType> RetTypes { get; } = retTypes;
}

public class LuaMethodType(LuaReturnType returnType, List<TypedParameter> parameters) : LuaType(TypeKind.Method)
{
    public LuaReturnType ReturnType { get; } = returnType;

    public List<TypedParameter> Parameters { get; } = parameters;
}
