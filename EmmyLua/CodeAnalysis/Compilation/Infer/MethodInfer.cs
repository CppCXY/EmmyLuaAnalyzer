using EmmyLua.CodeAnalysis.Compilation.Declaration;
using EmmyLua.CodeAnalysis.Compilation.Search;
using EmmyLua.CodeAnalysis.Compilation.Symbol;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;
using EmmyLua.CodeAnalysis.Type;


namespace EmmyLua.CodeAnalysis.Compilation.Infer;

public static class MethodInfer
{
    enum SignatureMatchType
    {
        DoNotMatch,
        ParamPartialMatch,
        ParamCountMatch,
        EmitterStyle
    }

    struct SignatureMatchResult
    {
        public static readonly SignatureMatchResult NotMatch = new()
        {
            MatchType = SignatureMatchType.DoNotMatch,
            MatchCount = 0
        };

        public SignatureMatchType MatchType { get; set; }
        public int MatchCount { get; set; }

        public static bool operator <(SignatureMatchResult left, SignatureMatchResult right)
        {
            if (left.MatchType < right.MatchType)
            {
                return true;
            }

            if (left.MatchType == right.MatchType)
            {
                return left.MatchCount < right.MatchCount;
            }

            return false;
        }

        public static bool operator >(SignatureMatchResult left, SignatureMatchResult right)
        {
            if (left.MatchType > right.MatchType)
            {
                return true;
            }

            if (left.MatchType == right.MatchType)
            {
                return left.MatchCount > right.MatchCount;
            }

            return false;
        }

        public static bool operator <=(SignatureMatchResult left, SignatureMatchResult right)
        {
            return !(left > right);
        }

        public static bool operator >=(SignatureMatchResult left, SignatureMatchResult right)
        {
            return !(left < right);
        }
    }

    public static LuaSignature FindPerfectMatchSignature(
        LuaMethodType methodType,
        LuaCallExprSyntax callExpr,
        List<LuaExprSyntax> args,
        SearchContext context)
    {
        if (methodType is LuaGenericMethodType genericMethodType)
        {
            return FindGenericPerfectMatchSignature(genericMethodType, callExpr, args, context);
        }

        if (methodType.Overloads is null)
        {
            return methodType.MainSignature;
        }

        var maxMatch = Match(methodType.MainSignature, callExpr, args, context, methodType.ColonDefine);
        LuaSignature? perfectMatch = methodType.MainSignature;
        foreach (var signature in methodType.Overloads)
        {
            var match = Match(signature, callExpr, args, context, methodType.ColonDefine);
            if (match > maxMatch)
            {
                maxMatch = match;
                perfectMatch = signature;
            }
        }

        return perfectMatch;
    }

    private static LuaSignature FindGenericPerfectMatchSignature(
        LuaGenericMethodType methodType,
        LuaCallExprSyntax callExpr,
        List<LuaExprSyntax> args,
        SearchContext context)
    {
        var signatures = methodType.GetInstantiatedSignatures(callExpr, args, context);

        var maxMatch = SignatureMatchResult.NotMatch;
        LuaSignature perfectMatch = signatures.First();
        foreach (var signature in signatures)
        {
            var match = Match(signature, callExpr, args, context, methodType.ColonDefine);
            if (match > maxMatch)
            {
                maxMatch = match;
                perfectMatch = signature;
            }
        }

        return perfectMatch;
    }

    // 返回值表示匹配程度, 匹配程度只是表征对lua来讲有多少个参数匹配了, 但是并不表示类型完全匹配
    // lua没有重载决议, 所以只要参数数量接近就可以了
    // 考虑到emitter风格重载, 最多比较第一个参数的类型
    private static SignatureMatchResult Match(LuaSignature signature, LuaCallExprSyntax callExpr,
        List<LuaExprSyntax> args, SearchContext context,
        bool colonDefine)
    {
        var colonCall = false;
        if (callExpr.PrefixExpr is LuaIndexExprSyntax indexExpr)
        {
            colonCall = indexExpr.IsColonIndex;
        }

        switch ((colonCall, colonDefine))
        {
            case (true, false):
            {
                return InnerMatch(signature, args, context, 1);
            }
            case (false, true):
            {
                LuaType selfType = Builtin.Any;
                if (callExpr.PrefixExpr is LuaIndexExprSyntax { PrefixExpr: { } prefixExpr })
                {
                    selfType = context.Infer(prefixExpr);
                }

                if (args.Count == 0)
                {
                    return SignatureMatchResult.NotMatch;
                }

                var matchedParam = 0;
                var firstArgType = context.Infer(args[0]);
                if (firstArgType.SubTypeOf(selfType, context))
                {
                    matchedParam++;
                }

                return InnerMatch(signature, args.Skip(1).ToList(), context, 0, matchedParam);
            }
            default:
            {
                return InnerMatch(signature, args, context, 0);
            }
        }
    }

    private static SignatureMatchResult InnerMatch(LuaSignature signature, List<LuaExprSyntax> args,
        SearchContext context, int skipParam,
        int matchedParam = 0)
    {
        var matchResult = new SignatureMatchResult
        {
            MatchType = SignatureMatchType.ParamPartialMatch,
            MatchCount = matchedParam
        };

        if (skipParam >= signature.Parameters.Count || args.Count == 0)
        {
            return matchResult;
        }

        var firstType = signature.Parameters[skipParam].Type;
        if (firstType is LuaStringLiteralType stringLiteralType && args[0] is LuaLiteralExprSyntax
            {
                Literal: LuaStringToken stringToken
            })
        {
            if (stringLiteralType.Content == stringToken.Value)
            {
                matchResult.MatchType = SignatureMatchType.EmitterStyle;
                return matchResult;
            }
        }
        else if (firstType is LuaIntegerLiteralType integerLiteralType && args[0] is LuaLiteralExprSyntax
                 {
                     Literal: LuaIntegerToken integer
                 })
        {
            if (integerLiteralType.Value == integer.Value)
            {
                matchResult.MatchType = SignatureMatchType.EmitterStyle;
                return matchResult;
            }
        }

        var paramCount = signature.Parameters.Count - skipParam;
        var argsCount = args.Count;
        if (paramCount == argsCount)
        {
            matchResult.MatchType = SignatureMatchType.ParamCountMatch;
            matchResult.MatchCount = argsCount;
        }
        else
        {
            matchResult.MatchCount = Math.Min(paramCount, argsCount);
        }

        return matchResult;
    }

    public static LuaSignature InstantiateSignature(
        LuaSignature signature,
        LuaCallExprSyntax callExpr,
        List<LuaExprSyntax> args,
        Dictionary<string, LuaType> genericParams,
        bool colonDefine,
        SearchContext context)
    {
        var colonCall = false;
        var typeSubstitution = new TypeSubstitution();
        typeSubstitution.SetTemplate(genericParams);
        if (callExpr.PrefixExpr is LuaIndexExprSyntax indexExpr)
        {
            colonCall = indexExpr.IsColonIndex;
        }

        switch ((colonCall, colonDefine))
        {
            case (true, false):
            {
                return InnerInstantiate(signature, callExpr, args, typeSubstitution, 1, 0, context);
            }
            case (false, true):
            {
                LuaType selfType = Builtin.Any;
                if (callExpr.PrefixExpr is LuaIndexExprSyntax { PrefixExpr: { } prefixExpr })
                {
                    selfType = context.Infer(prefixExpr);
                }

                if (args.Count == 0)
                {
                    return signature;
                }

                var matchedParam = 0;
                var firstArgType = context.Infer(args[0]);
                if (firstArgType.SubTypeOf(selfType, context))
                {
                    matchedParam++;
                }

                return InnerInstantiate(signature, callExpr, args, typeSubstitution, 0, matchedParam,
                    context);
            }
            default:
            {
                return InnerInstantiate(signature, callExpr, args, typeSubstitution, 0, 0, context);
            }
        }
    }

    private static LuaSignature InnerInstantiate(
        LuaSignature signature,
        LuaCallExprSyntax callExpr,
        List<LuaExprSyntax> args,
        TypeSubstitution substitution,
        int skipParam,
        int matchedParam,
        SearchContext context)
    {
        var newParameters = new List<LuaSymbol>();
        if (skipParam == 1)
        {
            if (args.Count > 0 && callExpr.PrefixExpr is LuaIndexExprSyntax { PrefixExpr: { } callSelf })
            {
                var prefixType = context.Infer(callSelf);
                var parameterType = signature.Parameters[0].Type;
                GenericInfer.InferByType(parameterType, prefixType, substitution,
                    context);
            }

            newParameters.Add(signature.Parameters[0].Instantiate(substitution));
        }

        var paramStart = skipParam;
        var argStart = matchedParam;

        for (var i = 0;
             i + paramStart < signature.Parameters.Count
             && i + argStart < args.Count
             && !substitution.InferFinished;
             i++)
        {
            var parameter = signature.Parameters[i + paramStart];
            if (parameter is
                {
                    Info: ParamInfo { IsVararg: true, DeclarationType: LuaExpandType expandType }
                })
            {
                var varargs = args[(i + argStart)..];
                GenericInfer.InferByExpandTypeAndExprs(expandType, varargs, substitution, context);
            }
            else
            {
                var arg = args[i + argStart];
                var parameterType = parameter.Type;
                GenericInfer.InferByExpr(parameterType, arg, substitution,
                    context);
            }
        }

        substitution.AnalyzeDefaultType();
        var newReturnType = signature.ReturnType.Instantiate(substitution);
        foreach (var parameter in signature.Parameters)
        {
            if (parameter.Type is LuaExpandType expandType)
            {
                newParameters.AddRange(substitution.GetSpreadParameters(expandType.Name));
            }
            else
            {
                newParameters.Add(parameter.Instantiate(substitution));
            }
        }

        return new LuaSignature(newReturnType, newParameters);
    }
}
