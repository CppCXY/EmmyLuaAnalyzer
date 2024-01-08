using EmmyLuaAnalyzer.CodeAnalysis.Compilation.Analyzer.Declaration;
using EmmyLuaAnalyzer.CodeAnalysis.Compilation.Analyzer.Infer;
using EmmyLuaAnalyzer.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace EmmyLuaAnalyzer.CodeAnalysis.Compilation.Type;

public class LuaMethod(MethodSignature mainSignature, List<MethodSignature>? overloadSignatures)
    : LuaType(TypeKind.Method)
{
    public MethodSignature MainSignature { get; } = mainSignature;

    public List<MethodSignature> OverloadSignatures { get; } = overloadSignatures ?? [];

    public void ProcessSignature(Func<MethodSignature, bool> process)
    {
        if (!process(MainSignature))
        {
            return;
        }

        foreach (var signature in OverloadSignatures)
        {
            if (!process(signature))
            {
                break;
            }
        }
    }

    public MethodSignature? FindPerfectSignature(
        IEnumerable<LuaExprSyntax> arguments,
        SearchContext context)
    {
        MethodSignature? perfectSignature = null;
        var perfectCount = 0;
        var argumentsList = arguments.ToList();
        ProcessSignature(signature =>
        {
            var count = signature.Match(argumentsList, context);

            if (count > perfectCount)
            {
                perfectSignature = signature;
                perfectCount = count;
            }

            return true;
        });

        return perfectSignature;
    }

    public override IEnumerable<Declaration> GetMembers(SearchContext context) => Enumerable.Empty<Declaration>();

    protected override ILuaType OnSubstitute(SearchContext context)
    {
        var same = true;
        var mainSignature = MainSignature.Substitute(context);
        if (!ReferenceEquals(MainSignature, mainSignature))
        {
            same = false;
        }

        var overloadSignatures = OverloadSignatures.Select(it =>
        {
            var substitute = it.Substitute(context);
            if (!ReferenceEquals(it, substitute))
            {
                same = false;
            }

            return substitute;
        }).ToList();
        return !same ? new LuaMethod(mainSignature, overloadSignatures) : this;
    }

    public override bool SubTypeOf(ILuaType other, SearchContext context)
    {
        var otherSubstitute = other.Substitute(context);
        if (ReferenceEquals(this, otherSubstitute))
        {
            return true;
        }

        if (otherSubstitute is LuaMethod method)
        {
            return MainSignature.SubTypeOf(method.MainSignature, context);
        }

        return false;
    }
}

public class MethodSignature(bool colonCall, List<Declaration> parameters, ILuaType? retType)
{
    public bool ColonCall { get; } = colonCall;

    public ILuaType? ReturnType { get; } = retType;

    public List<Declaration> Parameters { get; } = parameters;

    public ILuaType? Variadic
    {
        get
        {
            if (Parameters.LastOrDefault() is { Name: "..." } lastParam)
            {
                return lastParam.FirstDeclaration.Type;
            }

            return null;
        }
    }

    public int Match(List<LuaExprSyntax> arguments, SearchContext context)
    {
        var matched = 0;
        for (; matched < Parameters.Count; matched++)
        {
            if (arguments.Count <= matched)
            {
                return matched;
            }

            var arg = arguments[matched];
            var param = Parameters[matched];
            if (param.Type is { } type)
            {
                if (!type.AcceptExpr(arg, context))
                {
                    return matched;
                }
            }
        }

        if (arguments.Count > matched && Variadic != null)
        {
            for (; matched < arguments.Count; matched++)
            {
                var arg = arguments[matched];
                if (!Variadic.AcceptExpr(arg, context))
                {
                    return matched;
                }
            }
        }

        return matched;
    }

    public MethodSignature Substitute(SearchContext context)
    {
        var same = true;
        var parameters = new List<Declaration>();
        foreach (var parameter in Parameters)
        {
            var type = parameter.Type;
            if (type != null)
            {
                var substitute = type.Substitute(context);
                if (ReferenceEquals(substitute, type))
                {
                    parameters.Add(parameter);
                }
                else
                {
                    parameters.Add(parameter.WithType(substitute));
                    same = false;
                }
            }
            else
            {
                parameters.Add(parameter);
            }
        }

        var retType = ReturnType?.Substitute(context);
        if (!ReferenceEquals(ReturnType, retType))
        {
            same = false;
        }

        return !same ? new MethodSignature(ColonCall, parameters, retType) : this;
    }

    public bool SubTypeOf(MethodSignature other, SearchContext context)
    {
        if (ReferenceEquals(this, other))
        {
            return true;
        }

        if (other.ColonCall != ColonCall)
        {
            return false;
        }

        for (var i = 0; i < Parameters.Count; i++)
        {
            var luaType = Parameters[i].Type;
            var type = other.Parameters[i].Type;
            if (type != null && luaType != null && !luaType.SubTypeOf(type, context))
            {
                return false;
            }
        }

        return true;
    }
}
