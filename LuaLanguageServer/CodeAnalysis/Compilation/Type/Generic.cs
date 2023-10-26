using LuaLanguageServer.CodeAnalysis.Compilation.Infer;

namespace LuaLanguageServer.CodeAnalysis.Compilation.Type;

public class Generic : LuaType
{
    public Generic() : base(TypeKind.Generic)
    {
    }

    public override IEnumerable<LuaTypeMember> GetMembers(SearchContext context)
    {
        throw new NotImplementedException();
    }
}
