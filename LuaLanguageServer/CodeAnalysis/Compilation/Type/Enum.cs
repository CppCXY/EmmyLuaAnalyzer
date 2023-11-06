using LuaLanguageServer.CodeAnalysis.Compilation.Infer;
using LuaLanguageServer.CodeAnalysis.Compilation.StubIndex;
using LuaLanguageServer.CodeAnalysis.Syntax.Node;

namespace LuaLanguageServer.CodeAnalysis.Compilation.Type;

public class Enum : LuaType, ILuaNamedType
{
    public string Name { get; }

    public LuaSyntaxElement? GetSyntaxElement(SearchContext context) => context.Compilation
        .StubIndexImpl.ShortNameIndex.Get<LuaShortName.Enum>(Name).FirstOrDefault()?.EnumSyntax;

    public IEnumerable<GenericParam> GetGenericParams(SearchContext context)
    {
        return Enumerable.Empty<GenericParam>();
    }

    public Enum(string name) : base(TypeKind.Enum)
    {
        Name = name;
    }

    public override IEnumerable<EnumMember> GetMembers(SearchContext context)
    {
        var syntaxElement = context.Compilation
            .StubIndexImpl.ShortNameIndex.Get<LuaShortName.Enum>(Name).FirstOrDefault()?.EnumSyntax;
        if (syntaxElement is null)
        {
            yield break;
        }

        foreach (var field in syntaxElement.FieldList)
        {
            if (field.Name?.RepresentText is { } name)
            {
                var member = context.InferMember(field, () => new EnumMember(name, this));
                if (member is not null)
                {
                    yield return member;
                }
            }
        }

        // var memberIndex = context.Compilation.StubIndexImpl.Members;
        // TODO attach variable
    }

    public ILuaType GetBaseType(SearchContext context)
    {
        return context.Compilation.Builtin.Integer;
    }
}

public class EnumMember : LuaTypeMember
{
    public string Name { get; }

    public EnumMember(string name, Enum containingType) : base(containingType)
    {
        Name = name;
    }

    public override ILuaType GetType(SearchContext context)
    {
        return (ContainingType as Enum)?.GetBaseType(context) ?? context.Compilation.Builtin.Unknown;
    }

    public override bool MatchKey(IndexKey key, SearchContext context)
    {
        return key is IndexKey.String { Value: { } name } && name == Name;
    }
}
