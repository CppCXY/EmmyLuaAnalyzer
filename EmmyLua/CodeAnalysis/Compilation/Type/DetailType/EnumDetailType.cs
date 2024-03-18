using EmmyLua.CodeAnalysis.Compilation.Infer;

namespace EmmyLua.CodeAnalysis.Compilation.Type.DetailType;

public class EnumDetailType(string name, SearchContext context) : BasicDetailType(name, NamedTypeKind.Enum, context)
{
    private LuaType? _baseType = null;

    public LuaType? BaseType
    {
        get
        {
            if (!LazyInit)
            {
                DoLazyInit();
            }

            return _baseType;
        }
    }

    protected override void DoLazyInit()
    {
        base.DoLazyInit();
        _baseType = Index.GetSupers(Name).FirstOrDefault();
    }
}
