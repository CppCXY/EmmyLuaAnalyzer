using EmmyLua.CodeAnalysis.Compilation.Declaration;
using EmmyLua.CodeAnalysis.Compilation.Infer;

namespace EmmyLua.CodeAnalysis.Compilation.Type.DetailType;

public class InterfaceDetailType(
    string name, SearchContext context) : BasicDetailType(name, NamedTypeKind.Interface, context)
{
    private List<LuaType>? _supers;

    private List<GenericParameterLuaDeclaration>? _generics;

    private NamedTypeLuaDeclaration? _declaration;


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

    public List<GenericParameterLuaDeclaration> Generics
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

    public NamedTypeLuaDeclaration? Declaration
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
