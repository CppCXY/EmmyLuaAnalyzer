namespace EmmyLua.CodeAnalysis.Compilation.Type.Types;

public class LuaUnionType(IEnumerable<LuaType> unionTypes)
    : LuaType
{
    public List<LuaType> UnionTypes { get; } = unionTypes.ToList();

    // public override LuaType Instantiate(TypeSubstitution substitution)
    // {
    //     var newUnionTypes = UnionTypes.Select(t => t.Instantiate(substitution));
    //     return new LuaUnionType(newUnionTypes);
    // }
}

public class LuaTupleType(List<LuaType> typeList)
    : LuaType
{
    public List<LuaType> TypeList { get; } = typeList;

    // public override LuaType Instantiate(TypeSubstitution substitution)
    // {
    //     var newTupleTypes = TypeList
    //         .Select(t => t.Instantiate(substitution))
    //         .ToList();
    //     return new LuaTupleType(newTupleTypes);
    // }
}

public class LuaArrayType(LuaType baseType)
    : LuaType
{
    public LuaType BaseType { get; } = baseType;

    // public override LuaType Instantiate(TypeSubstitution substitution)
    // {
    //     var newBaseType = BaseType.Instantiate(substitution);
    //     return new LuaArrayType(newBaseType);
    // }
}

public class LuaVariadicType(LuaType baseType)
    : LuaType
{
    public LuaType BaseType { get; } = baseType;

    // public override LuaType Instantiate(TypeSubstitution substitution)
    // {
    //     var newBaseType = BaseType.Instantiate(substitution);
    //     return new LuaVariadicType(newBaseType);
    // }
}

public class InstanceType(LuaType baseType)
    : LuaType
{
    public LuaType BaseType { get; } = baseType;

    // public override LuaType Instantiate(TypeSubstitution substitution)
    // {
    //     var newBaseType = BaseType.Instantiate(substitution);
    //     return new InstanceType(newBaseType);
    // }
}

public class EnumInstanceType(LuaNamedType enumType, LuaType? baseType)
    : LuaType
{
    public LuaNamedType EnumType { get; } = enumType;

    public LuaType BaseType { get; } = baseType ?? Builtin.Integer;
}

