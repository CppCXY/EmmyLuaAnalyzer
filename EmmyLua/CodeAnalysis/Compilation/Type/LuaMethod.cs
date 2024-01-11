using EmmyLua.CodeAnalysis.Compilation.Analyzer.Declaration;
using EmmyLua.CodeAnalysis.Compilation.Analyzer.Infer;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace EmmyLua.CodeAnalysis.Compilation.Type;

public class LuaMethod(
    bool colon,
    List<Declaration> parameters,
    ILuaType? returnType,
    List<LuaTypeRef>? overloads = null)
    : LuaType(TypeKind.Method)
{
    public List<LuaTypeRef>? Overloads { get; } = overloads;

    public bool ColonCall { get; } = colon;

    public ILuaType? ReturnType { get; } = returnType;

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

    public void ProcessSignature(Func<LuaMethod, bool> process, SearchContext context)
    {
        if (!process(this))
        {
            return;
        }

        if (Overloads is not null)
        {
            foreach (var signature in Overloads.Select(it => it.Substitute(context)).Cast<LuaMethod>())
            {
                if (!process(signature))
                {
                    break;
                }
            }
        }
    }

    public LuaMethod FindPerfectSignature(
        IEnumerable<LuaExprSyntax> arguments,
        SearchContext context)
    {
        var perfectSignature = this;
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
        }, context);

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
        var otherSubstitute = other.Substitute(context);

        if (ReferenceEquals(this, otherSubstitute))
        {
            return true;
        }

        if (otherSubstitute is LuaMethod otherMethod)
        {
            if (otherMethod.ColonCall != ColonCall)
            {
                return false;
            }

            for (var i = 0; i < Parameters.Count; i++)
            {
                var luaType = Parameters[i].Type;
                var type = otherMethod.Parameters[i].Type;
                if (type != null && luaType != null && !luaType.SubTypeOf(type, context))
                {
                    return false;
                }
            }
        }

        return true;
    }
}
