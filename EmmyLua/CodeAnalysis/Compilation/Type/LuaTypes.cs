using EmmyLua.CodeAnalysis.Compilation.Infer;

namespace EmmyLua.CodeAnalysis.Compilation.Type;

public class LuaType(TypeKind kind) : IEquatable<LuaType>
{
    public TypeKind Kind { get; } = kind;

    public override bool Equals(object? obj)
    {
        return Equals(obj as LuaType);
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

    public virtual bool SubTypeOf(LuaType? other, SearchContext context)
    {
        if (other is null)
        {
            return false;
        }

        if (other.Kind is TypeKind.Any or TypeKind.Unknown)
        {
            return true;
        }

        return Equals(other);
    }

    public virtual LuaType Instantiate(Dictionary<string, LuaType> genericReplace)
    {
        return this;
    }
}

public class LuaNamedType(string name, TypeKind kind = TypeKind.NamedType) : LuaType(kind), IEquatable<LuaNamedType>
{
    public string Name { get; } = name;

    public override bool Equals(object? obj)
    {
        return Equals(obj as LuaNamedType);
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

        if (!base.Equals(other))
        {
            return false;
        }

        return string.Equals(Name, other.Name, StringComparison.CurrentCulture);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(base.GetHashCode(), Name);
    }

    public override bool SubTypeOf(LuaType? other, SearchContext context)
    {
        if (Equals(other))
        {
            return true;
        }

        if (other is not LuaNamedType namedType)
        {
            return false;
        }

        var supers = context.Compilation.ProjectIndex.GetSupers(Name);
        return supers.Any(super => super.SubTypeOf(namedType, context));
    }

    public override LuaType Instantiate(Dictionary<string, LuaType> genericReplace)
    {
        return genericReplace.TryGetValue(Name, out var type) ? type : this;
    }
}

public class LuaUnionType(IEnumerable<LuaType> unionTypes) : LuaType(TypeKind.Union)
{
    public HashSet<LuaType> UnionTypes { get; } = [..unionTypes];

    public override bool SubTypeOf(LuaType? other, SearchContext context)
    {
        if (Equals(other))
        {
            return true;
        }

        if (other is not LuaUnionType unionType)
        {
            return false;
        }

        return UnionTypes.All(t => unionType.UnionTypes.Any(ut => t.SubTypeOf(ut, context)));
    }

    public override LuaType Instantiate(Dictionary<string, LuaType> genericReplace)
    {
        var newUnionTypes = UnionTypes.Select(t => t.Instantiate(genericReplace));
        return new LuaUnionType(newUnionTypes);
    }
}

public class LuaTupleType(List<LuaType> tupleTypes) : LuaType(TypeKind.Tuple), IEquatable<LuaTupleType>
{
    public List<LuaType> TupleTypes { get; } = tupleTypes;

    public bool Equals(LuaTupleType? other)
    {
        if (ReferenceEquals(this, other)) return true;
        return base.Equals(other) && TupleTypes.Equals(other.TupleTypes);
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as LuaTupleType);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(base.GetHashCode(), TupleTypes);
    }

    public override bool SubTypeOf(LuaType? other, SearchContext context)
    {
        if (Equals(other))
        {
            return true;
        }

        if (other is not LuaTupleType tupleType)
        {
            return false;
        }

        if (TupleTypes.Count != tupleType.TupleTypes.Count)
        {
            return false;
        }

        return !TupleTypes.Where((t, i) => !t.SubTypeOf(tupleType.TupleTypes[i], context)).Any();
    }

    public override LuaType Instantiate(Dictionary<string, LuaType> genericReplace)
    {
        var newTupleTypes = TupleTypes.Select(t => t.Instantiate(genericReplace)).ToList();
        return new LuaTupleType(newTupleTypes);
    }
}

public class LuaArrayType(LuaType baseType) : LuaType(TypeKind.Array), IEquatable<LuaArrayType>
{
    public LuaType BaseType { get; } = baseType;

    public bool Equals(LuaArrayType? other)
    {
        if (ReferenceEquals(this, other)) return true;
        return base.Equals(other) && BaseType.Equals(other.BaseType);
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as LuaArrayType);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(base.GetHashCode(), BaseType);
    }

    public override bool SubTypeOf(LuaType? other, SearchContext context)
    {
        if (Equals(other))
        {
            return true;
        }

        if (other is not LuaArrayType arrayType)
        {
            return false;
        }

        return BaseType.SubTypeOf(arrayType.BaseType, context);
    }

    public override LuaType Instantiate(Dictionary<string, LuaType> genericReplace)
    {
        var newBaseType = BaseType.Instantiate(genericReplace);
        return new LuaArrayType(newBaseType);
    }
}

public class LuaGenericType(string baseName, List<LuaType> genericArgs)
    : LuaNamedType(baseName, TypeKind.Generic), IEquatable<LuaGenericType>
{
    public List<LuaType> GenericArgs { get; } = genericArgs;

    public bool Equals(LuaGenericType? other)
    {
        if (ReferenceEquals(this, other)) return true;
        return base.Equals(other) && GenericArgs.Equals(other.GenericArgs);
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as LuaGenericType);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(base.GetHashCode(), GenericArgs);
    }

    public override LuaType Instantiate(Dictionary<string, LuaType> genericReplace)
    {
        var newName = Name;
        if (genericReplace.TryGetValue(Name, out var type))
        {
            if (type is LuaNamedType namedType)
            {
                newName = namedType.Name;
            }
        }

        var newGenericArgs = GenericArgs.Select(t => t.Instantiate(genericReplace)).ToList();
        return new LuaGenericType(newName, newGenericArgs);
    }
}

public class LuaStringLiteralType(string content) : LuaType(TypeKind.StringLiteral), IEquatable<LuaStringLiteralType>
{
    public string Content { get; } = content;

    public bool Equals(LuaStringLiteralType? other)
    {
        if (ReferenceEquals(this, other)) return true;
        return base.Equals(other) && Content == other.Content;
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as LuaStringLiteralType);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(base.GetHashCode(), Content);
    }

    public override bool SubTypeOf(LuaType? other, SearchContext context)
    {
        return Equals(other);
    }
}

public class LuaIntegerLiteralType(long value) : LuaType(TypeKind.IntegerLiteral), IEquatable<LuaIntegerLiteralType>
{
    public long Value { get; } = value;

    public bool Equals(LuaIntegerLiteralType? other)
    {
        if (ReferenceEquals(this, other)) return true;
        return base.Equals(other) && Value == other.Value;
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as LuaIntegerLiteralType);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(base.GetHashCode(), Value);
    }

    public override bool SubTypeOf(LuaType? other, SearchContext context)
    {
        return Equals(other);
    }
}

public class LuaTableLiteralType(string tableId)
    : LuaNamedType(tableId, TypeKind.TableLiteral), IEquatable<LuaTableLiteralType>
{
    public string TableId { get; } = tableId;

    public bool Equals(LuaTableLiteralType? other)
    {
        if (ReferenceEquals(this, other)) return true;
        return base.Equals(other) && TableId == other.TableId;
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as LuaTableLiteralType);
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }
}
