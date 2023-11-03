using LuaLanguageServer.CodeAnalysis.Compilation.Infer;
using LuaLanguageServer.CodeAnalysis.Syntax.Node;

namespace LuaLanguageServer.CodeAnalysis.Compilation.Type;

public class GenericImpl : LuaType, ILuaNamedType
{
    public string Name { get; }

    public LuaSyntaxElement? GetSyntaxElement(SearchContext context)
    {
        throw new NotImplementedException();
    }

    public Dictionary<string, ILuaType> GenericImplParameters { get; }

    public GenericImpl(string name, Dictionary<string, ILuaType> genericImplParameters) : base(TypeKind.Generic)
    {
        GenericImplParameters = genericImplParameters;
        Name = name;
    }

    public override IEnumerable<GenericImplMember> GetMembers(SearchContext context)
    {
        // throw new NotImplementedException();
        // 找到name的class或者接口 wrapper其成员
        throw new NotImplementedException();
    }
}

public class GenericImplMember : LuaTypeMember
{
    public IndexKey Key { get; }

    public LuaSyntaxElement SyntaxElement { get; }

    public GenericImplMember(IndexKey key, LuaSyntaxElement syntaxElement, GenericImpl containingType) : base(
        containingType)
    {
        Key = key;
        SyntaxElement = syntaxElement;
    }

    public override ILuaType GetType(SearchContext context)
    {
        throw new NotImplementedException();
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
