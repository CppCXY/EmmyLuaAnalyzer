using LuaLanguageServer.CodeAnalysis.Compilation.Infer;
using LuaLanguageServer.CodeAnalysis.Syntax.Node;
using LuaLanguageServer.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace LuaLanguageServer.CodeAnalysis.Compilation.Type;

public class GenericParam : LuaType, ILuaNamedType
{
    public string Name { get; }

    public ILuaType? Type { get; }

    private LuaDocGenericParamSyntax _genericParamSyntax;

    public GenericParam(string name, ILuaType? type, LuaDocGenericParamSyntax genericParamSyntax)
        : base(TypeKind.GenericParam)
    {
        Name = name;
        Type = type;
        _genericParamSyntax = genericParamSyntax;
    }

    public LuaSyntaxElement? GetSyntaxElement(SearchContext context) => _genericParamSyntax;

    public override IEnumerable<LuaTypeMember> GetMembers(SearchContext context)
    {
        return Type?.GetMembers(context) ?? Enumerable.Empty<LuaTypeMember>();
    }
}
