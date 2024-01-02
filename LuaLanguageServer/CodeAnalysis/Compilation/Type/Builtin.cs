using LuaLanguageServer.CodeAnalysis.Compilation.Analyzer.Declaration;
using LuaLanguageServer.CodeAnalysis.Compilation.Analyzer.Infer;
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
}

public class PrimitiveLuaClass(string name) : LuaClass(name);

public class Unknown() : Primitive("unknown");

