using EmmyLua.CodeAnalysis.Compilation.Type;
using EmmyLua.CodeAnalysis.Compilation.Type.Types;

namespace EmmyLua.CodeAnalysis.Compilation.Search;

public class Operators(SearchContext context)
{
    public IEnumerable<TypeOperator> GetOperators(TypeOperatorKind kind, LuaNamedType left)
    {
        var typeInfo = context.Compilation.TypeManager.FindTypeInfo(left);
        if (typeInfo is null)
        {
            return [];
        }

        if (typeInfo.Operators is null)
        {
            return [];
        }

        if (typeInfo.Operators.TryGetValue(kind, out var operators))
        {
            if (left is LuaGenericType genericType && typeInfo.GenericParams is not null)
            {
                var substitution = new TypeSubstitution();
                var genericArgs = genericType.GenericArgs;
                for (var i = 0; i < typeInfo.GenericParams.Count && i < genericArgs.Count; i++)
                {
                    substitution.Add(typeInfo.GenericParams[i].Name, genericArgs[i], true);
                }

                var instanceOperators = operators.Select(op => op.Instantiate(substitution)).ToList();
                return instanceOperators;
            }

            return operators;
        }

        return [];
    }
}
