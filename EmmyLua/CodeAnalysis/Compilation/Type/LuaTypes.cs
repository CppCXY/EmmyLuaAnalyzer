namespace EmmyLua.CodeAnalysis.Compilation.Type;

public class LuaType(TypeKind kind)
{
    public TypeKind Kind { get; } = kind;
}

public class LuaNamedType(string name, TypeKind kind) : LuaType(kind)
{
    public virtual string Name { get; } = name;
}

public class LuaNilType() : LuaType(TypeKind.Nil);

public class LuaUnionType() : LuaType(TypeKind.Union)
{
    private List<LuaType> UnionTypes { get; } = new();

    public IEnumerable<LuaType> Types => UnionTypes;

    public void AddType(LuaType type)
    {
        UnionTypes.Add(type);
    }
}

public class LuaAliasType(string name, LuaType type) : LuaNamedType(name, TypeKind.Alias)
{
    public LuaType Type { get; } = type;
}

public class LuaTupleType(List<LuaType> tupleTypes) : LuaType(TypeKind.Tuple)
{
    public List<LuaType> TupleTypes { get; } = tupleTypes;
}

public class LuaArrayType(LuaType baseType) : LuaType(TypeKind.Array)
{
    public LuaType BaseType { get; } = baseType;
}

public class LuaGenericType(LuaType baseType, List<LuaType> genericArgs) : LuaType(TypeKind.Generic)
{
    public LuaType BaseType { get; } = baseType;

    public List<LuaType> GenericArgs { get; } = genericArgs;
}

public class LuaStringLiteralType(string content) : LuaType(TypeKind.StringLiteral)
{
    public string Content { get; } = content;
}

public class LuaIntegerLiteralType(int value) : LuaType(TypeKind.IntegerLiteral)
{
    public long Value { get; } = value;
}

