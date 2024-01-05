using LuaLanguageServer.CodeAnalysis.Compilation.Analyzer.Declaration;
using LuaLanguageServer.CodeAnalysis.Compilation.Analyzer.Infer;
using LuaLanguageServer.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace LuaLanguageServer.CodeAnalysis.Compilation.Type;

public class LuaMethod(bool colonCall, List<Declaration> parameters, ILuaType? retType)
    : LuaType(TypeKind.Method)
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

    public static void ProcessSignature(Func<LuaMethod, bool> process, IEnumerable<LuaMethod> signatures)
    {
        foreach (var signature in signatures)
        {
            if (!process(signature))
            {
                break;
            }
        }
    }

    public static LuaMethod? FindPerfectSignature(
        IEnumerable<LuaExprSyntax> arguments,
        IEnumerable<LuaMethod> signatures,
        SearchContext context)
    {
        LuaMethod? perfectSignature = null;
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
        }, signatures);

        return perfectSignature;
    }

    public override IEnumerable<Declaration> GetMembers(SearchContext context) => Enumerable.Empty<Declaration>();

    protected override ILuaType OnSubstitute(SearchContext context)
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

        return !same ? new LuaMethod(ColonCall, parameters, retType) : this;
    }

    public override bool SubTypeOf(ILuaType other, SearchContext context)
    {
        if (ReferenceEquals(this, other))
        {
            return true;
        }

        if (other is LuaMethod method)
        {
            if (method.ColonCall != ColonCall)
            {
                return false;
            }

            if (method.Parameters.Count != Parameters.Count)
            {
                return false;
            }

            for (var i = 0; i < Parameters.Count; i++)
            {
                var luaType = Parameters[i].Type;
                var type = method.Parameters[i].Type;
                if (type != null && luaType != null && !luaType.SubTypeOf(type, context))
                {
                    return false;
                }
            }

            if (method.Variadic != null && Variadic != null && !Variadic.SubTypeOf(method.Variadic, context))
            {
                return false;
            }

            if (method.ReturnType != null && ReturnType != null && !ReturnType.SubTypeOf(method.ReturnType, context))
            {
                return false;
            }

            return true;
        }

        return false;
    }
}
