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

    public IEnumerable<GenericParam> GetGenericParams(SearchContext context)
    {
        return Enumerable.Empty<GenericParam>();
    }

    public override IEnumerable<LuaTypeMember> GetMembers(SearchContext context)
    {
        return Type?.GetMembers(context) ?? Enumerable.Empty<LuaTypeMember>();
    }
}

public class Generic : LuaType, IGeneric
{
    public ILuaNamedType BaseType { get; }

    public List<ILuaType> GenericArgs { get; }

    private Dictionary<string, ILuaType>? _genericImpl;

    public Generic(ILuaNamedType baseType, List<ILuaType> genericArgs) : base(TypeKind.Generic)
    {
        BaseType = baseType;
        GenericArgs = genericArgs;
    }

    public override IEnumerable<LuaTypeMember> GetMembers(SearchContext context)
    {
        return BaseType.GetMembers(context);
    }

    public override IEnumerable<LuaTypeMember> IndexMember(IndexKey key, SearchContext context)
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

public class GenericTypeMember : LuaTypeMember
{
    public LuaTypeMember Member { get; }

    public Generic Parent { get; }

    public GenericTypeMember(LuaTypeMember member, Generic parent) : base(parent)
    {
        Member = member;
        Parent = parent;
    }

    public override ILuaType? GetType(SearchContext context)
    {
        context.PushEnv(Parent.GetGenericIml(context));
        var ty =  Member.GetType(context);
        context.PopEnv();
        return ty;
    }

    public override bool MatchKey(IndexKey key, SearchContext context)
    {
        return Member.MatchKey(key, context);
    }
}
