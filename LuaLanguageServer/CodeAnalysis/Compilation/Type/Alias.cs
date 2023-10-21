using LuaLanguageServer.CodeAnalysis.Compilation.Infer;

namespace LuaLanguageServer.CodeAnalysis.Compilation.Type;

public class Alias : LuaType, ILuaNamedType
{
    public string Name { get; }

    public Alias(string name) : base(TypeKind.Alias)
    {
        Name = name;
    }

    public override IEnumerable<ILuaType> GetMembers(SearchContext context)
    {
        throw new NotImplementedException();
    }
}
