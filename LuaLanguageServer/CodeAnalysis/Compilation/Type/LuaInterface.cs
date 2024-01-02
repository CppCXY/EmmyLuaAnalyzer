using LuaLanguageServer.CodeAnalysis.Compilation.Analyzer.Declaration;
using LuaLanguageServer.CodeAnalysis.Compilation.Analyzer.Infer;

namespace LuaLanguageServer.CodeAnalysis.Compilation.Type;

public class LuaInterface(string name) : LuaType(TypeKind.Interface), IGenericBase
{
    public string Name { get; } = name;

    public override IEnumerable<Declaration> GetMembers(SearchContext context)
    {
        return context.FindMembers(Name);
    }

    public override bool SubTypeOf(ILuaType other, SearchContext context)
    {
        return ReferenceEquals(this, other) ||
               other is LuaInterface @interface && string.Equals(Name, @interface.Name, StringComparison.CurrentCulture);
    }
}
