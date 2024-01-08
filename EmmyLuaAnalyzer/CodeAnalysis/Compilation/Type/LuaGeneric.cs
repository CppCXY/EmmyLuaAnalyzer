using EmmyLuaAnalyzer.CodeAnalysis.Compilation.Analyzer.Declaration;
using EmmyLuaAnalyzer.CodeAnalysis.Compilation.Analyzer.Infer;

namespace EmmyLuaAnalyzer.CodeAnalysis.Compilation.Type;

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
        foreach (var memberDeclaration in BaseType.GetMembers(context))
        {
            if (memberDeclaration.Type is { } ty)
            {
                yield return memberDeclaration.WithType(ty.Substitute(context, GetGenericEnv(context)));
            }
            else
            {
                yield return memberDeclaration;
            }
        }
    }

    public override IEnumerable<Declaration> IndexMember(string name, SearchContext context)
    {
        foreach (var memberDeclaration in BaseType.IndexMember(name, context))
        {
            if (memberDeclaration.Type is { } ty)
            {
                yield return memberDeclaration.WithType(ty.Substitute(context, GetGenericEnv(context)));
            }
            else
            {
                yield return memberDeclaration;
            }
        }
    }

    public override IEnumerable<Declaration> IndexMember(long index, SearchContext context)
    {
        foreach (var memberDeclaration in BaseType.IndexMember(index, context))
        {
            if (memberDeclaration.Type is { } ty)
            {
                yield return memberDeclaration.WithType(ty.Substitute(context, GetGenericEnv(context)));
            }
            else
            {
                yield return memberDeclaration;
            }
        }
    }

    public override IEnumerable<Declaration> IndexMember(ILuaType ty, SearchContext context)
    {
        foreach (var memberDeclaration in BaseType.IndexMember(ty, context))
        {
            if (memberDeclaration.Type is { } ty2)
            {
                yield return memberDeclaration.WithType(ty2.Substitute(context, GetGenericEnv(context)));
            }
            else
            {
                yield return memberDeclaration;
            }
        }
    }

    private Dictionary<string, ILuaType> GetGenericEnv(SearchContext context)
    {
        var env = new Dictionary<string, ILuaType>();
        var genericParams = BaseType.GetGenericParams(context).ToList();
        for (var i = 0; i < genericParams.Count; i++)
        {
            if (i < GenericArgs.Count)
            {
                env[genericParams[i].Name] = GenericArgs[i];
            }
            else if (genericParams[i].Type is { } ty)
            {
                env[genericParams[i].Name] = ty;
            }
        }

        return env;
    }
}
