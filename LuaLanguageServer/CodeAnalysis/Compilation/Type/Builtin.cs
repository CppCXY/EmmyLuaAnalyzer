using LuaLanguageServer.CodeAnalysis.Compilation.Infer;
using LuaLanguageServer.CodeAnalysis.Compilation.Symbol;
using LuaLanguageServer.CodeAnalysis.Syntax.Location;

namespace LuaLanguageServer.CodeAnalysis.Compilation.Type;

public class Builtin
{
    public readonly Unknown Unknown = new Unknown();
    public readonly Primitive Void = new Primitive("void");
    public readonly Primitive Nil = new Primitive("nil");
    public readonly Primitive Number = new Primitive("number");
    public readonly Primitive Integer = new Primitive("integer");
    public readonly PrimitiveLuaClass String = new PrimitiveLuaClass("string");
    public readonly Primitive Boolean = new Primitive("boolean");
    public readonly Primitive Userdata = new Primitive("userdata");
    public readonly PrimitiveLuaClass Io = new PrimitiveLuaClass("io");
    public readonly PrimitiveLuaClass Table = new PrimitiveLuaClass("table");
    public readonly PrimitiveLuaClass Global = new PrimitiveLuaClass("global");
    public readonly PrimitiveLuaClass Iter = new PrimitiveLuaClass("iter");

    public Builtin()
    {
        Integer.WithSuper(Number);
    }

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
            "_G" => Global,
            _ => null
        };
    }
}

public class Primitive : LuaClass
{
    public ILuaType? Super { get; private set; }

    public Primitive(string name) : base(name)
    {
    }

    public override ILuaType? GetSuper(SearchContext context)
    {
        return Super;
    }

    public Primitive WithSuper(ILuaType super)
    {
        Super = super;
        return this;
    }

    public override IEnumerable<ILuaSymbol> GetMembers(SearchContext context) => Enumerable.Empty<LuaSymbol>();

    public override IEnumerable<ILuaSymbol> IndexMember(IndexKey key, SearchContext context) =>
        Enumerable.Empty<LuaSymbol>();
}

public class PrimitiveLuaClass : LuaClass
{
    public PrimitiveLuaClass(string name) : base(name)
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


    public override IEnumerable<ILuaSymbol> GetMembers(SearchContext context)
    {
        return Enumerable.Empty<LuaSymbol>();
    }

    public override IEnumerable<ILuaSymbol> IndexMember(IndexKey key, SearchContext context)
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

public class PrimitiveGenericTableMember : LuaSymbol
{
    public ILuaType Type { get; }

    public PrimitiveGenericTableMember(ILuaType type, ILuaType? containingType) : base(containingType)
    {
        Type = type;
    }

    public override ILuaType GetType(SearchContext context)
    {
        return Type;
    }

    public override IEnumerable<LuaLocation> GetLocations(SearchContext context)
    {
        throw new NotImplementedException();
    }
}
