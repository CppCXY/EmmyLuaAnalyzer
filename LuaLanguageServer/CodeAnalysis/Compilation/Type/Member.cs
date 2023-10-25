using LuaLanguageServer.CodeAnalysis.Compilation.Infer;
using LuaLanguageServer.CodeAnalysis.Compilation.StubIndex;
using LuaLanguageServer.CodeAnalysis.Syntax.Node;
using LuaLanguageServer.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace LuaLanguageServer.CodeAnalysis.Compilation.Type;

public abstract class LuaTypeMember : LuaType
{
    public ILuaType? ContainingType { get; }

    public LuaTypeMember(ILuaType? containingType) : base(TypeKind.Field)
    {
        ContainingType = containingType;
    }

    public override IEnumerable<ILuaType> GetMembers(SearchContext context)
    {
        return GetType(context)?.GetMembers(context) ?? Enumerable.Empty<ILuaType>();
    }

    public abstract ILuaType? GetType(SearchContext context);
}

public class EnumMember : LuaTypeMember, ILuaNamedType
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
}

public class InterfaceMember : LuaTypeMember, ILuaIndexedType
{
    public IndexKey Key { get; }

    public LuaSyntaxElement SyntaxElement { get; }

    public InterfaceMember(IndexKey key, LuaSyntaxElement syntaxElement, Interface containingType) : base(containingType)
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
}

public class ClassMember : LuaTypeMember, ILuaIndexedType
{
    public IndexKey Key { get; }

    public LuaSyntaxElement SyntaxElement { get; }

    public ClassMember(IndexKey key, LuaSyntaxElement syntaxElement, Class containingType) : base(containingType)
    {
        Key = key;
        SyntaxElement = syntaxElement;
    }

    public override ILuaType? GetType(SearchContext context)
    {
        return SyntaxElement switch
        {
            LuaDocTypedFieldSyntax typeField => context.Infer(typeField.Type),
            LuaDocFieldSyntax field => context.Infer(field.Type),
            _ => null
        };
    }
}

