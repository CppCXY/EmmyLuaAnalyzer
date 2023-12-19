using LuaLanguageServer.CodeAnalysis.Compilation.Analyzer.Declaration;
using LuaLanguageServer.CodeAnalysis.Compilation.Analyzer.Infer;
using LuaLanguageServer.CodeAnalysis.Compilation.Symbol;
using LuaLanguageServer.CodeAnalysis.Syntax.Node;
using LuaLanguageServer.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace LuaLanguageServer.CodeAnalysis.Compilation.Type;

public class GenericParam(string name, ILuaType? type, LuaDocTagGenericParamSyntax tagGenericParamSyntax)
    : LuaType(TypeKind.GenericParam), ILuaNamedType
{
    public string Name { get; } = name;

    public ILuaType? Type { get; } = type;

    public LuaSyntaxElement? GetSyntaxElement(SearchContext context) => tagGenericParamSyntax;

    public IEnumerable<GenericParam> GetGenericParams(SearchContext context)
    {
        return Enumerable.Empty<GenericParam>();
    }

    public override IEnumerable<Declaration> GetMembers(SearchContext context)
    {
        return Type?.GetMembers(context) ?? Enumerable.Empty<Declaration>();
    }
}

public class LuaGeneric(ILuaNamedType baseType, List<ILuaType> genericArgs) : LuaType(TypeKind.Generic), IGeneric
{
    public ILuaNamedType BaseType { get; } = baseType;

    public List<ILuaType> GenericArgs { get; } = genericArgs;

    private Dictionary<string, ILuaType>? _genericImpl;

    public override IEnumerable<Declaration> GetMembers(SearchContext context)
    {
        return BaseType.GetMembers(context);
    }

    public override IEnumerable<Declaration> IndexMember(IndexKey key, SearchContext context)
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
