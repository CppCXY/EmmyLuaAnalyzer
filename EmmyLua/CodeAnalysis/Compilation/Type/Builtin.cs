using EmmyLua.CodeAnalysis.Compilation.Infer;
using EmmyLua.CodeAnalysis.Compilation.Symbol;

namespace EmmyLua.CodeAnalysis.Compilation.Type;

public class Builtin
{
    public readonly Unknown Unknown = new Unknown();
    public readonly Primitive Void = new Primitive("void");
    public readonly Primitive Nil = new Nil();
    public readonly Primitive Number = new Primitive("number");
    public readonly Primitive Integer = new Primitive("integer");
    public readonly LuaClass String = new LuaClass("string");
    public readonly Primitive Boolean = new Primitive("boolean");
    public readonly Primitive Userdata = new Primitive("userdata");
    public readonly LuaClass Io = new LuaClass("io");
    public readonly LuaTable Table = new LuaTable(string.Empty);
    public readonly LuaClass Global = new LuaClass("global");

    public Builtin()
    {
        Integer.WithSuper(Number);
    }

    public ILuaNamedType? FromName(string name)
    {
        return name switch
        {
            "unknown" => Unknown,
            "void" => Void,
            "nil" => Nil,
            "number" => Number,
            "integer" or "int" => Integer,
            "string" => String,
            "boolean" or "bool" => Boolean,
            "userdata" => Userdata,
            "io" => Io,
            "table" => Table,
            "_G" or "global" => Global,
            _ => null
        };
    }
}

public class Primitive(string name) : LuaClass(name)
{
    public ILuaType? Super { get; private set; }

    public Primitive WithSuper(ILuaType super)
    {
        Super = super;
        return this;
    }

    public override bool SubTypeOf(ILuaType other, SearchContext context) =>
        ReferenceEquals(this, other) || Super?.SubTypeOf(other, context) == true;
}

public class Unknown() : Primitive("unknown")
{
    public override bool IsNullable { get; } = true;

    public override bool SubTypeOf(ILuaType other, SearchContext context) => true;
}

public class Nil() : Primitive("nil")
{
    public override bool IsNullable { get; } = true;

    public override bool SubTypeOf(ILuaType other, SearchContext context) =>
        ReferenceEquals(this, other) || other.IsNullable;
}

public class LuaTable(string uniqueId) : LuaType(TypeKind.Table), IGenericBase
{
    public string Name { get; } = uniqueId;

    public override bool SubTypeOf(ILuaType other, SearchContext context)
    {
        var otherSubstitute = other.Substitute(context);
        return otherSubstitute is ILuaNamedType;
    }
}

public class GenericTable(ILuaType key, ILuaType value) : LuaTable(string.Empty), IGenericImpl
{
    public ILuaType Key { get; } = key;
    public ILuaType Value { get; } = value;

    public override bool SubTypeOf(ILuaType other, SearchContext context)
    {
        var otherSubstitute = other.Substitute(context);
        if (otherSubstitute is GenericTable { Key: { } key, Value: { } value })
        {
            return Key.SubTypeOf(key, context) && Value.SubTypeOf(value, context);
        }

        return base.SubTypeOf(other, context);
    }

    public IGenericBase GetBaseType(SearchContext context)
    {
        return context.Compilation.Builtin.Table;
    }

    public List<ILuaType> GenericArgs { get; } = [key, value];
}
