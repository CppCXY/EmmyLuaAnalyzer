using LuaLanguageServer.CodeAnalysis.Compilation.Infer;
using LuaLanguageServer.CodeAnalysis.Compilation.StubIndex;
using LuaLanguageServer.CodeAnalysis.Syntax.Node;
using LuaLanguageServer.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace LuaLanguageServer.CodeAnalysis.Compilation.Type;

public class Alias : LuaType, ILuaNamedType
{
    public string Name { get; }

    public Alias(string name) : base(TypeKind.Alias)
    {
        Name = name;
    }

    public override IEnumerable<LuaTypeMember> GetMembers(SearchContext context)
    {
        var syntaxElement = context.Compilation
            .StubIndexImpl.ShortNameIndex.Get<LuaShortName.Alias>(Name).FirstOrDefault()?.AliasSyntax;
        return syntaxElement is null ? Enumerable.Empty<LuaTypeMember>() : context.Infer(syntaxElement.Type).GetMembers(context);
    }

    public override IEnumerable<LuaTypeMember> IndexMember(IndexKey key, SearchContext context)
    {
        var syntaxElement = context.Compilation
            .StubIndexImpl.ShortNameIndex.Get<LuaShortName.Alias>(Name).FirstOrDefault()?.AliasSyntax;
        return syntaxElement is null ? Enumerable.Empty<LuaTypeMember>() : context.Infer(syntaxElement.Type).IndexMember(key, context);
    }
}
