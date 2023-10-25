using LuaLanguageServer.CodeAnalysis.Compilation.Infer;
using LuaLanguageServer.CodeAnalysis.Compilation.StubIndex;
using LuaLanguageServer.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace LuaLanguageServer.CodeAnalysis.Compilation.Type;

public class Interface : LuaType, ILuaNamedType
{
    public string Name { get; }

    public Interface(string name) : base(TypeKind.Interface)
    {
        Name = name;
    }

    public override IEnumerable<InterfaceMember> GetMembers(SearchContext context)
    {
        var syntaxElement = context.Compilation
            .StubIndexImpl.ShortNameIndex.Get<LuaShortName.Interface>(Name).FirstOrDefault()?.InterfaceSyntax;
        if (syntaxElement is null)
        {
            yield break;
        }

        var fields = context.Compilation.StubIndexImpl.Members.Get<LuaMember.InterfaceDocField>(syntaxElement);
        foreach (var field in fields)
        {

        }
    }

    public override IEnumerable<ILuaType> IndexMember(IndexKey key, SearchContext context)
    {
        throw new NotImplementedException();
        // return GetMembers(context)
    }
}
