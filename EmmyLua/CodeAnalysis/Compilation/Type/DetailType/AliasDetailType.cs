using EmmyLua.CodeAnalysis.Compilation.Infer;

namespace EmmyLua.CodeAnalysis.Compilation.Type.DetailType;

public class AliasDetailType(string name, SearchContext context) : BasicDetailType(name, NamedTypeKind.Alias, context)
{
    private LuaType? _originType;

    public LuaType? OriginType
    {
        get
        {
            if (!LazyInit)
            {
                DoLazyInit();
            }

            return _originType;
        }
    }

    protected override void DoLazyInit()
    {
        base.DoLazyInit();
        _originType = Index.GetAliasOriginType(Name).FirstOrDefault();
    }
}
