using EmmyLuaAnalyzer.CodeAnalysis.Compilation.Analyzer.Declaration;
using EmmyLuaAnalyzer.CodeAnalysis.Compilation.Analyzer.Infer;

namespace EmmyLuaAnalyzer.CodeAnalysis.Compilation.Type;

public class Builtin
{
    public readonly Unknown Unknown = new Unknown();
    public readonly Primitive Void = new Primitive("void");
    public readonly Primitive Nil = new Primitive("nil");
    public readonly Primitive Number = new Primitive("number");
    public readonly Primitive Integer = new Primitive("integer");
    public readonly LuaClass String = new LuaClass("string");
    public readonly Primitive Boolean = new Primitive("boolean");
    public readonly Primitive Userdata = new Primitive("userdata");
    public readonly LuaClass Io = new LuaClass("io");
    public readonly LuaTable Table = new LuaTable(null, null);
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

public class Primitive(string name) : LuaClass(name)
{
    public ILuaType? Super { get; private set; }

    public Primitive WithSuper(ILuaType super)
    {
        Super = super;
        return this;
    }

    public override IEnumerable<Declaration> GetMembers(SearchContext context) => Enumerable.Empty<Declaration>();

    public override IEnumerable<Declaration> IndexMember(string name, SearchContext context) =>
        Enumerable.Empty<Declaration>();

    public override IEnumerable<Declaration> IndexMember(long index, SearchContext context) =>
        Enumerable.Empty<Declaration>();

    public override IEnumerable<Declaration> IndexMember(ILuaType ty, SearchContext context) =>
        Enumerable.Empty<Declaration>();

    public override bool SubTypeOf(ILuaType other, SearchContext context) =>
        ReferenceEquals(this, other) || Super?.SubTypeOf(other, context) == true;
}

public class Unknown() : Primitive("unknown");

public class LuaTable(ILuaType? key, ILuaType? value) : LuaClass("table"), IGenericImpl
{
    public ILuaType? Key { get; } = key;

    public ILuaType? Value { get; } = value;

    public IGenericBase BaseType => this;

    public List<ILuaType> GenericArgs { get; } = [];

    public VirtualDeclaration MemberDeclaration { get; } = new(value);

    public static LuaTable WithGeneric(ILuaType key, ILuaType value)
    {
        return new LuaTable(key, value);
    }

    public override IEnumerable<Declaration> GetMembers(SearchContext context) => Enumerable.Empty<Declaration>();

    public override IEnumerable<Declaration> IndexMember(string name, SearchContext context)
    {
        if (ReferenceEquals(Key, context.Compilation.Builtin.String))
        {
            yield return MemberDeclaration;
        }
    }

    public override IEnumerable<Declaration> IndexMember(long index, SearchContext context)
    {
        if (ReferenceEquals(Key, context.Compilation.Builtin.Integer)
            || ReferenceEquals(Key, context.Compilation.Builtin.Number))
        {
            yield return MemberDeclaration;
        }
    }

    public override IEnumerable<Declaration> IndexMember(ILuaType ty, SearchContext context)
    {
        if (Key is not null && ty.SubTypeOf(Key, context))
        {
            yield return MemberDeclaration;
        }
    }

    public override bool SubTypeOf(ILuaType other, SearchContext context) =>
        ReferenceEquals(this, other) || other is LuaTable;

    public override IEnumerable<ILuaType> GetSupers(SearchContext context) => Enumerable.Empty<ILuaType>();
}
