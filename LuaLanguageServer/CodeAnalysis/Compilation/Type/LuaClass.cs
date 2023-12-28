using LuaLanguageServer.CodeAnalysis.Compilation.Analyzer.Declaration;
using LuaLanguageServer.CodeAnalysis.Compilation.Analyzer.Infer;


namespace LuaLanguageServer.CodeAnalysis.Compilation.Type;

public class LuaClass(string name) : LuaType(TypeKind.Class), IGenericBase
{
    public string Name { get; } = name;

    public IEnumerable<Declaration> GetRawMembers(SearchContext context)
    {
        return context.FindMembers(this);
    }

    public override IEnumerable<Declaration> GetMembers(SearchContext context)
    {
        return GetRawMembers(context);
    }

    public virtual ILuaType? GetSuper(SearchContext context)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<LuaInterface> GetInterfaces(SearchContext context)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// contains all interfaces
    /// </summary>
    public IEnumerable<LuaInterface> GetAllInterface(SearchContext context)
    {
        throw new NotImplementedException();
    }

    public override bool SubTypeOf(ILuaType other, SearchContext context)
    {
        if (other is LuaClass otherClass)
        {
            return string.Equals(Name, otherClass.Name, StringComparison.CurrentCulture);

        }

        return false;
    }
}

