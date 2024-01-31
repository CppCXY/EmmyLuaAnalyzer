using EmmyLua.CodeAnalysis.Compilation.Infer;
using EmmyLua.CodeAnalysis.Compilation.Symbol;

namespace EmmyLua.CodeAnalysis.Compilation.Type;

public interface IGenericBase : ILuaNamedType
{
    public IEnumerable<GenericParameterDeclaration> GetGenericParams(SearchContext context)
    {
        return context.FindGenericParams(Name);
    }
}

public interface IGenericImpl : ILuaType
{
    public IGenericBase GetBaseType(SearchContext context);

    public List<ILuaType> GenericArgs { get; }
}

public class LuaGenericImpl(IGenericBase baseType, List<ILuaType> genericArgs) : LuaType(TypeKind.Generic), IGenericImpl
{
    public List<ILuaType> GenericArgs { get; } = genericArgs;

    public Dictionary<string, ILuaType> GetGenericEnv(SearchContext context)
    {
        var env = new Dictionary<string, ILuaType>();
        var genericParams = baseType.GetGenericParams(context).ToList();
        for (var i = 0; i < genericParams.Count; i++)
        {
            if (i < GenericArgs.Count)
            {
                env[genericParams[i].Name] = GenericArgs[i];
            }
            else if (genericParams[i].DeclarationType is { } ty)
            {
                env[genericParams[i].Name] = ty;
            }
        }

        return env;
    }

    public IGenericBase GetBaseType(SearchContext context)
    {
        return baseType;
    }
}
