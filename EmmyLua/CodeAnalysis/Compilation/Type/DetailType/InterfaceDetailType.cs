using EmmyLua.CodeAnalysis.Compilation.Declaration;
using EmmyLua.CodeAnalysis.Compilation.Infer;

namespace EmmyLua.CodeAnalysis.Compilation.Type.DetailType;

public class InterfaceDetailType(
    string name, SearchContext context) : BasicDetailType(name, NamedTypeKind.Interface, context)
{
    private List<LuaType>? _supers;

    private List<GenericParamDeclaration>? _generics;

    private NamedTypeDeclaration? _declaration;


    public List<LuaType> Supers
    {
        get
        {
            if (!LazyInit)
            {
                DoLazyInit();
            }

            return _supers!;
        }
    }

    public List<GenericParamDeclaration> Generics
    {
        get
        {
            if (!LazyInit)
            {
                DoLazyInit();
            }

            return _generics!;
        }
    }

    public NamedTypeDeclaration? Declaration
    {
        get
        {
            if (!LazyInit)
            {
                DoLazyInit();
            }

            return _declaration;
        }
    }

    protected override void DoLazyInit()
    {
        base.DoLazyInit();
        _supers = Index.GetSupers(Name).ToList();
        _generics = Index.GetGenericParams(Name).ToList();
        _declaration = Index.GetTypeLuaDeclaration(Name);
    }
}
