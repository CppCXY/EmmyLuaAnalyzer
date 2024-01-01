using LuaLanguageServer.CodeAnalysis.Compilation.Analyzer.Declaration;
using LuaLanguageServer.CodeAnalysis.Compilation.Analyzer.Infer;

namespace LuaLanguageServer.CodeAnalysis.Compilation.Type;

public interface IGenericBase : ILuaNamedType
{
    public IEnumerable<Declaration> GetGenericParams(SearchContext context)
    {
        return context.FindGenericParams(Name);
    }
}

public interface IGenericImpl : ILuaType
{
    public IGenericBase BaseType { get; }

    public List<ILuaType> GenericArgs { get; }
}

public class LuaGenericImpl(IGenericBase baseType, List<ILuaType> genericArgs) : LuaType(TypeKind.Generic), IGenericImpl
{
    public IGenericBase BaseType { get; } = baseType;

    public List<ILuaType> GenericArgs { get; } = genericArgs;

    public override IEnumerable<Declaration> GetMembers(SearchContext context)
    {
        return BaseType.GetMembers(context);
    }

    public override IEnumerable<Declaration> IndexMember(string name, SearchContext context)
    {
        return BaseType.IndexMember(name, context);
    }

    public IEnumerable<ILuaType> GetGenericArgs(SearchContext context)
    {
        return GenericArgs;
    }
}
