using EmmyLua.CodeAnalysis.Compilation.Type;

namespace EmmyLua.CodeAnalysis.Compilation.Search;

public class Operators(SearchContext context)
{
    private Dictionary<LuaType, Dictionary<TypeOperatorKind, List<TypeOperator>>> TypeOperatorCaches { get; } = new();

    public IEnumerable<TypeOperator> GetOperators(TypeOperatorKind kind, LuaNamedType left)
    {
        if (left is LuaGenericType genericType)
        {
            if (context.Features.Cache)
            {
                if (TypeOperatorCaches.TryGetValue(genericType, out var cache))
                {
                    if (cache.TryGetValue(kind, out var operators))
                    {
                        return operators;
                    }
                }
            }

            var originOperators = context.Compilation.Db.QueryTypeOperators(left.Name)
                .Where(it => it.Kind == kind).ToList();

            var genericParams = context.Compilation.Db.QueryGenericParams(genericType.Name).ToList();
            var genericArgs = genericType.GenericArgs;

            var substitution = new TypeSubstitution();
            for (var i = 0; i < genericParams.Count && i < genericArgs.Count; i++)
            {
                substitution.Add(genericParams[i].Name , genericArgs[i], true);
            }

            var instanceOperators = originOperators.Select(op => op.Instantiate(substitution)).ToList();
            if (context.Features.Cache)
            {
                if (!TypeOperatorCaches.TryGetValue(genericType, out var cache))
                {
                    cache = new Dictionary<TypeOperatorKind, List<TypeOperator>>();
                    TypeOperatorCaches.Add(genericType, cache);
                }

                cache[kind] = instanceOperators;
            }

            return instanceOperators;
        }

        return context.Compilation.Db.QueryTypeOperators(left.Name)
            .Where(it => it.Kind == kind);
    }
}
