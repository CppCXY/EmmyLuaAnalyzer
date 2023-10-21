using LuaLanguageServer.CodeAnalysis.Compilation.Infer;

namespace LuaLanguageServer.CodeAnalysis.Compilation.Type;

public class Generic : LuaType
{
    public Generic() : base(TypeKind.Generic)
    {
    }

    public override IEnumerable<ILuaType> GetMembers(SearchContext context)
    {
        throw new NotImplementedException();
    }
}
