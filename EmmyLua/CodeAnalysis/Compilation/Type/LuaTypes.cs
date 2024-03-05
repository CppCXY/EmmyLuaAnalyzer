using EmmyLua.CodeAnalysis.Compilation.Infer;

namespace EmmyLua.CodeAnalysis.Compilation.Type;

public class LuaType(TypeKind kind) : IEquatable<LuaType>
{
    public TypeKind Kind { get; } = kind;

    public LuaType Union(LuaType other)
    {
        if (Kind == TypeKind.Union)
        {
            var unionType = (LuaUnionType)this;
            return unionType.AddType(other);
        }
        else
        {
            return new LuaUnionType([this, other]);
        }
    }

    public LuaType Remove(LuaType other)
    {
        if (Kind == TypeKind.Union)
        {
            var unionType = (LuaUnionType)this;
            return unionType.RemoveType(other);
        }
        else
        {
            return this;
        }
    }

    public override bool Equals(object? obj)
    {
        return ReferenceEquals(this, obj);
    }

    public bool Equals(LuaType? other)
    {
        if (ReferenceEquals(this, other))
        {
            return true;
        }

        if (other is null)
        {
            return false;
        }

        return Kind == other.Kind;
    }

    public override int GetHashCode()
    {
        return (int)Kind;
    }
}

public class LuaNamedType(string name, TypeKind kind = TypeKind.NamedType) : LuaType(kind), IEquatable<LuaNamedType>
{
    public string Name { get; } = name;

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(this, obj))
        {
            return true;
        }

        if (obj is LuaNamedType other)
        {
            return Name == other.Name;
        }

        return false;
    }

    public bool Equals(LuaNamedType? other)
    {
        if (ReferenceEquals(this, other))
        {
            return true;
        }

        if (other is not null)
        {
            return Name == other.Name;
        }

        return false;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(base.GetHashCode(), Name);
    }
}

public class LuaNilType() : LuaNamedType("nil", TypeKind.Nil);

public class LuaUnionType(List<LuaType> unionTypes) : LuaType(TypeKind.Union)
{
    private List<LuaType> UnionTypes { get; } = unionTypes;

    public IEnumerable<LuaType> Types => UnionTypes;

    public LuaUnionType AddType(LuaType type)
    {
        var types = new List<LuaType>();
        types.AddRange(UnionTypes);
        types.Add(type);
        return new LuaUnionType(types);
    }

    public LuaUnionType AddTypes(IEnumerable<LuaType> types)
    {
        var newTypes = new List<LuaType>();
        newTypes.AddRange(UnionTypes);
        newTypes.AddRange(types);
        return new LuaUnionType(newTypes);
    }

    public LuaType RemoveType(LuaType type)
    {
        var types = new List<LuaType>();
        types.AddRange(UnionTypes);
        types.Remove(type);
        if (types.Count == 1)
        {
            return types.First();
        }
        else
        {
            return new LuaUnionType(types);
        }
    }
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




