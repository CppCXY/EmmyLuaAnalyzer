using LuaLanguageServer.CodeAnalysis.Compilation.Infer;
using LuaLanguageServer.CodeAnalysis.Compilation.Symbol;
using LuaLanguageServer.CodeAnalysis.Syntax.Node;
using LuaLanguageServer.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace LuaLanguageServer.CodeAnalysis.Compilation.Type;

public class GenericParam : LuaType, ILuaNamedType
{
    public string Name { get; }

    public ILuaType? Type { get; }

    private LuaDocTagGenericParamSyntax _tagGenericParamSyntax;

    public GenericParam(string name, ILuaType? type, LuaDocTagGenericParamSyntax tagGenericParamSyntax)
        : base(TypeKind.GenericParam)
    {
        Name = name;
        Type = type;
        _tagGenericParamSyntax = tagGenericParamSyntax;
    }

    public LuaSyntaxElement? GetSyntaxElement(SearchContext context) => _tagGenericParamSyntax;

    public IEnumerable<GenericParam> GetGenericParams(SearchContext context)
    {
        return Enumerable.Empty<GenericParam>();
    }

    public override IEnumerable<ILuaSymbol> GetMembers(SearchContext context)
    {
        return Type?.GetMembers(context) ?? Enumerable.Empty<LuaSymbol>();
    }
}

public class LuaGeneric : LuaType, IGeneric
{
    public ILuaNamedType BaseType { get; }

    public List<ILuaType> GenericArgs { get; }

    private Dictionary<string, ILuaType>? _genericImpl;

    public LuaGeneric(ILuaNamedType baseType, List<ILuaType> genericArgs) : base(TypeKind.Generic)
    {
        BaseType = baseType;
        GenericArgs = genericArgs;
    }

    public override IEnumerable<ILuaSymbol> GetMembers(SearchContext context)
    {
        return BaseType.GetMembers(context);
    }

    public override IEnumerable<ILuaSymbol> IndexMember(IndexKey key, SearchContext context)
    {
        return BaseType.IndexMember(key, context);
    }

    public ILuaNamedType GetBaseType(SearchContext context)
    {
        return BaseType;
    }

    public IEnumerable<ILuaType> GetGenericArgs(SearchContext context)
    {
        return GenericArgs;
    }

    public Dictionary<string, ILuaType> GetGenericIml(SearchContext context)
    {
        if (_genericImpl is null)
        {
            _genericImpl = new Dictionary<string, ILuaType>();
            var genericParams = BaseType.GetGenericParams(context).ToList();
            for (var i = 0; i < GenericArgs.Count; i++)
            {
                var arg = GenericArgs[i];
                if (i < genericParams.Count)
                {
                    var p = genericParams[i];
                    _genericImpl.Add(p.Name, arg);
                }
            }
        }

        return _genericImpl;
    }
}
