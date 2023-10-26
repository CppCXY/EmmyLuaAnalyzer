using LuaLanguageServer.CodeAnalysis.Compilation.Infer;

namespace LuaLanguageServer.CodeAnalysis.Compilation.Type;

public class Builtin
{
    public readonly Primitive Unknown = new Unknown();
    public readonly Primitive Void =new Primitive("void");
    public readonly Primitive Nil = new Primitive("nil");
    public readonly Primitive Number = new Primitive("number");
    public readonly Primitive Integer = new Primitive("integer");
    public readonly Primitive String = new Primitive("string");
    public readonly Primitive Boolean = new Primitive("boolean");
    public readonly Primitive Userdata = new Primitive("userdata");
    public readonly Primitive Io = new Primitive("io");
    public readonly Primitive Table = new Primitive("table");

    public ILuaType? FromName(string name)
    {
        return name switch
        {
            "nil" => Nil,
            "void" => Void,
            "number" => Number,
            "integer" => Integer,
            "string" => String,
            "boolean" => Boolean,
            _ => null
        };
    }
}

public class Primitive : LuaType, ILuaNamedType
{
    public Primitive(string name) : base(TypeKind.Primitive)
    {
        Name = name;
    }

    public IEnumerable<string> MemberNames => Enumerable.Empty<string>();

    public string Name { get; }

    public override IEnumerable<LuaTypeMember> GetMembers(SearchContext context) => Enumerable.Empty<LuaTypeMember>();

    public override IEnumerable<LuaTypeMember> IndexMember(IndexKey key, SearchContext context)
    {
        throw new NotImplementedException();
    }
}

public class Unknown : Primitive
{
    public Unknown() : base("unknown")
    {
    }
}
