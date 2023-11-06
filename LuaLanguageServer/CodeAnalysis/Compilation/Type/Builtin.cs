using LuaLanguageServer.CodeAnalysis.Compilation.Infer;

namespace LuaLanguageServer.CodeAnalysis.Compilation.Type;

public class Builtin
{
    public readonly Unknown Unknown = new Unknown();
    public readonly Primitive Void = new Primitive("void");
    public readonly Primitive Nil = new Primitive("nil");
    public readonly Primitive Number = new Primitive("number");
    public readonly Primitive Integer = new Primitive("integer");
    public readonly PrimitiveClass String = new PrimitiveClass("string");
    public readonly Primitive Boolean = new Primitive("boolean");
    public readonly Primitive Userdata = new Primitive("userdata");
    public readonly PrimitiveClass Io = new PrimitiveClass("io");
    public readonly PrimitiveClass Table = new PrimitiveClass("table");

    public ILuaType? FromName(string name)
    {
        return name switch
        {
            "unknown" => Unknown,
            "void" => Void,
            "nil" => Nil,
            "number" => Number,
            "integer" => Integer,
            "string" => String,
            "boolean" => Boolean,
            "userdata" => Userdata,
            "io" => Io,
            "table" => Table,
            _ => null
        };
    }
}

public class Primitive : Class
{
    public Primitive(string name) : base(name)
    {
    }

    public override IEnumerable<ClassMember> GetMembers(SearchContext context) => Enumerable.Empty<ClassMember>();

    public override IEnumerable<ClassMember> IndexMember(IndexKey key, SearchContext context) =>
        Enumerable.Empty<ClassMember>();
}

public class PrimitiveClass : Class
{
    public PrimitiveClass(string name) : base(name)
    {
    }
}

public class Unknown : Primitive
{
    public Unknown() : base("unknown")
    {
    }
}

public class PrimitiveGenericTable : LuaType, IGeneric
{
    public ILuaType KeyType { get; }

    public ILuaType ValueType { get; }

    public PrimitiveGenericTableMember MemberType { get; }

    public PrimitiveGenericTable(ILuaType keyType, ILuaType valueType) : base(TypeKind.GenericTable)
    {
        KeyType = keyType;
        ValueType = valueType;
        MemberType = new PrimitiveGenericTableMember(valueType, this);
    }


    public override IEnumerable<LuaTypeMember> GetMembers(SearchContext context)
    {
        return Enumerable.Empty<LuaTypeMember>();
    }

    public override IEnumerable<LuaTypeMember> IndexMember(IndexKey key, SearchContext context)
    {
        switch (key)
        {
            case IndexKey.Integer:
            {
                if (KeyType.SubTypeOf(context.Compilation.Builtin.Number, context))
                {
                    yield return MemberType;
                }

                break;
            }
            case IndexKey.String:
            {
                if (KeyType.SubTypeOf(context.Compilation.Builtin.String, context))
                {
                    yield return MemberType;
                }

                break;
            }
            case IndexKey.Ty ty:
            {
                if (ty.Value.SubTypeOf(KeyType, context))
                {
                    yield return MemberType;
                }

                break;
            }
        }
    }

    public ILuaNamedType GetBaseType(SearchContext context)
    {
        return context.Compilation.Builtin.Table;
    }

    public IEnumerable<ILuaType> GetGenericArgs(SearchContext context)
    {
        yield return KeyType;
        yield return ValueType;
    }
}

public class PrimitiveGenericTableMember : LuaTypeMember
{
    public ILuaType Type { get; }

    public PrimitiveGenericTableMember(ILuaType type, ILuaType? containingType) : base(containingType)
    {
        Type = type;
    }

    public override ILuaType? GetType(SearchContext context)
    {
        return Type;
    }

    public override bool MatchKey(IndexKey key, SearchContext context)
    {
        return true;
    }
}
