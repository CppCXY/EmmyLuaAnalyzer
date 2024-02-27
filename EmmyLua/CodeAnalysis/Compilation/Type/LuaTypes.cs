using EmmyLua.CodeAnalysis.Compilation.Infer;

namespace EmmyLua.CodeAnalysis.Compilation.Type;

public class LuaType(TypeKind kind)
{
    public TypeKind Kind { get; } = kind;
}

public class LuaNamedType(string name, TypeKind kind = TypeKind.NamedType) : LuaType(kind)
{
    public string Name { get; } = name;
}

public class LuaNilType() : LuaNamedType("nil", TypeKind.Nil);

public class LuaUnionType(List<LuaType> unionTypes) : LuaType(TypeKind.Union)
{
    private List<LuaType> UnionTypes { get; } = unionTypes;

    public IEnumerable<LuaType> Types => UnionTypes;

    public void AddType(LuaType type)
    {
        UnionTypes.Add(type);
    }

    public void AddTypes(IEnumerable<LuaType> types)
    {
        UnionTypes.AddRange(types);
    }

    public void RemoveType(LuaType type)
    {
        UnionTypes.Remove(type);
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

public class LuaGenericType(string baseName, List<LuaType> genericArgs) : LuaNamedType(baseName, TypeKind.Generic)
{
    public List<LuaType> GenericArgs { get; } = genericArgs;
}

public class LuaStringLiteralType(string content) : LuaType(TypeKind.StringLiteral)
{
    public string Content { get; } = content;
}

public class LuaIntegerLiteralType(long value) : LuaType(TypeKind.IntegerLiteral)
{
    public long Value { get; } = value;
}

public class LuaTableLiteralType(string tableId) : LuaNamedType(tableId, TypeKind.TableLiteral)
{
    public string TableId { get; } = tableId;
}

public class TypedParameter(string name, LuaType? type)
{
    public string Name { get; } = name;
    public LuaType? Type { get; } = type;
}

public class LuaMethodType(List<LuaType> retTypes, List<TypedParameter> parameters) : LuaType(TypeKind.Method)
{
    public List<LuaType> RetTypes { get; } = retTypes;

    public LuaType? FirstRetType => RetTypes.FirstOrDefault();

    public List<TypedParameter> Parameters { get; } = parameters;
}
