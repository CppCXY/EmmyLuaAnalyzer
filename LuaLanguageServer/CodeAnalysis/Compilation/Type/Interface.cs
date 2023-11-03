using LuaLanguageServer.CodeAnalysis.Compilation.Infer;
using LuaLanguageServer.CodeAnalysis.Compilation.StubIndex;
using LuaLanguageServer.CodeAnalysis.Syntax.Node;
using LuaLanguageServer.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace LuaLanguageServer.CodeAnalysis.Compilation.Type;

public class Interface : LuaType, ILuaNamedType
{
    public string Name { get; }

    public LuaSyntaxElement? GetSyntaxElement(SearchContext context) => context.Compilation
        .StubIndexImpl.ShortNameIndex.Get<LuaShortName.Interface>(Name).FirstOrDefault()?.InterfaceSyntax;

    public Interface(string name) : base(TypeKind.Interface)
    {
        Name = name;
    }

    public override IEnumerable<InterfaceMember> GetMembers(SearchContext context)
    {
        var syntaxElement = GetSyntaxElement(context);
        if (syntaxElement is null)
        {
            yield break;
        }

        var fields = context.Compilation.StubIndexImpl.Members.Get<LuaMember.InterfaceDocField>(syntaxElement);
        foreach (var field in fields)
        {
        }
    }
}

public class InterfaceMember : LuaTypeMember
{
    public IndexKey Key { get; }

    public LuaSyntaxElement SyntaxElement { get; }

    public InterfaceMember(IndexKey key, LuaSyntaxElement syntaxElement, Interface containingType) : base(
        containingType)
    {
        Key = key;
        SyntaxElement = syntaxElement;
    }

    public override ILuaType? GetType(SearchContext context)
    {
        if (SyntaxElement is LuaDocTypedFieldSyntax typeField)
        {
            return context.Infer(typeField.Type);
        }

        return null;
    }

    public override bool MatchKey(IndexKey key, SearchContext context)
    {
        return (key, Key) switch
        {
            (IndexKey.Integer i1, IndexKey.Integer i2) => i1.Value == i2.Value,
            (IndexKey.String s1, IndexKey.String s2) => s1.Value == s2.Value,
            (IndexKey.Ty t1, IndexKey.Ty t2) => t1.Value == t2.Value,
            _ => false
        };
    }
}
