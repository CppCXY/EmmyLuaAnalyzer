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
