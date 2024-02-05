using System.Text;
using EmmyLua.CodeAnalysis.Compilation.Infer;
using EmmyLua.CodeAnalysis.Compilation.Symbol;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace EmmyLua.CodeAnalysis.Compilation.Type;

public class LuaMethod(
    Signature mainSignature,
    List<Signature>? overloadSignatures = null,
    List<GenericParameterDeclaration>? genericParameters = null
    )
    : LuaType(TypeKind.Method)
{
    public Signature MainSignature { get; } = mainSignature;

    public List<GenericParameterDeclaration>? GenericParameters { get; } = genericParameters;

    public List<Signature>? OverloadSignatures { get; } = overloadSignatures;

    public ILuaType? SelfType { get; internal set; }

    public void ProcessSignature(Func<Signature, bool> process, SearchContext context)
    {
        if (!process(MainSignature))
        {
            return;
        }

        if (OverloadSignatures is not null)
        {
            foreach (var signature in OverloadSignatures)
            {
                if (!process(signature))
                {
                    break;
                }
            }
        }
    }

    private static int MatchCount(List<ParameterDeclaration> parameters, List<LuaExprSyntax> arguments, SearchContext context)
    {
        var matched = 0;
        for (; matched < parameters.Count; matched++)
        {
            if (arguments.Count <= matched)
            {
                return matched;
            }

            var arg = arguments.ElementAtOrDefault(matched);
            var argTy = context.Infer(arg);
            var param = parameters[matched];
            if (param.DeclarationType is { } type)
            {
                if (!argTy.SubTypeOf(type, context))
                {
                    return matched;
                }
            }
        }

        return matched;
    }

    public Signature FindPerfectSignature(
        LuaCallExprSyntax callExpr,
        SearchContext context)
    {
        var isColonCall = false;
        if (callExpr.PrefixExpr is LuaIndexExprSyntax indexExpr)
        {
            isColonCall = indexExpr.IsColonIndex;
        }

        var args = callExpr.ArgList?.ArgList.ToList() ?? [];
        var perfectSignature = MainSignature;
        var perfectCount = 0;
        ProcessSignature(signature =>
        {
            var count = 0;
            var isColonDefine = signature.ColonDefine;
            switch ((isColonCall, isColonDefine))
            {
                case (true, false):
                {
                    if (signature.Parameters.FirstOrDefault() is { Name: "self" })
                    {
                        count++;
                        count += MatchCount(signature.Parameters.Skip(1).ToList(), args, context);
                    }

                    break;
                }
                case (false, true):
                {
                    var declarations = new List<ParameterDeclaration> { ParameterDeclaration.SelfParameter(SelfType) };
                    declarations.AddRange(signature.Parameters);
                    count += MatchCount(declarations, args, context);
                    break;
                }
                case (true, true):
                {
                    count++;
                    count += MatchCount(signature.Parameters, args, context);
                    break;
                }
            }

            if (count > perfectCount)
            {
                perfectSignature = signature;
                perfectCount = count;
            }

            return true;
        }, context);

        return perfectSignature;
    }

    protected override bool OnSubTypeOf(ILuaType other, SearchContext context)
    {
        var otherSubstitute = other.Substitute(context);
        if (ReferenceEquals(this, otherSubstitute))
        {
            return true;
        }

        if (otherSubstitute is LuaMethod method)
        {
            if (MainSignature.SubTypeOf(method.MainSignature, context))
            {
                return true;
            }
        }

        return false;
    }

    public override string ToDisplayString(SearchContext context)
    {
        return MainSignature.ToDisplayString(context);
    }
}

public class Signature(
    bool colon,
    List<ParameterDeclaration> parameters,
    ILuaType? returnTypes)
{
    public bool ColonDefine { get; } = colon;

    public ILuaType? ReturnTypes { get; internal set; } = returnTypes;

    public List<ParameterDeclaration> Parameters { get; } = parameters;

    public ILuaType? Variadic
    {
        get
        {
            if (Parameters.LastOrDefault() is { Name: "..." } lastParam)
            {
                return lastParam.FirstSymbol.DeclarationType;
            }

            return null;
        }
    }

    public bool SubTypeOf(Signature other, SearchContext context)
    {
        if (ReferenceEquals(this, other))
        {
            return true;
        }

        if (other.ColonDefine != ColonDefine)
        {
            return false;
        }

        for (var i = 0; i < Parameters.Count; i++)
        {
            var luaType = Parameters[i].DeclarationType;
            var type = other.Parameters.ElementAtOrDefault(i)?.DeclarationType;
            if (type != null && luaType != null && !luaType.SubTypeOf(type, context))
            {
                return false;
            }
        }

        return true;
    }

    public string ToDisplayString(SearchContext context)
    {
        var sb = new StringBuilder();
        sb.Append("fun(");

        var first = true;
        if (ColonDefine)
        {
            sb.Append("self");
            first = false;
        }

        foreach (var parameter in Parameters)
        {
            if (!first)
            {
                sb.Append(", ");
            }

            first = false;
            sb.Append($"{parameter.Name}: {parameter.DeclarationType?.ToDisplayString(context) ?? "any"}");
        }

        sb.Append(')');
        if (ReturnTypes != null)
        {
            sb.Append("=> ");
            sb.Append(ReturnTypes.ToDisplayString(context));
        }

        return sb.ToString();
    }
}
