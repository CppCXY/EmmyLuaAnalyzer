using EmmyLua.CodeAnalysis.Compilation.Declaration;
using EmmyLua.CodeAnalysis.Compilation.Infer;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace EmmyLua.CodeAnalysis.Compilation.Type;

public enum SignatureMatchType
{
    DoNotMatch,
    ParamPartialMatch,
    ParamCountMatch,
    EmitterStyle
}

public struct SignatureMatchResult
{
    public static readonly SignatureMatchResult NotMatch = new SignatureMatchResult
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

public class LuaSignature(LuaType returnType, List<LuaDeclaration> parameters) : IEquatable<LuaSignature>
{
    public LuaType ReturnType { get; set; } = returnType;

    public List<LuaDeclaration> Parameters { get; } = parameters;

    // 返回值表示匹配程度, 匹配程度只是表征对lua来讲有多少个参数匹配了, 但是并不表示类型完全匹配
    // lua没有重载决议, 所以只要参数数量接近就可以了
    // 考虑到emitter风格重载, 最多比较第一个参数的类型
    public SignatureMatchResult Match(LuaCallExprSyntax callExpr, List<LuaExprSyntax> args, SearchContext context,
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
                return InnerMatch(args, context, 1);
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

                return InnerMatch(args.Skip(1).ToList(), context, 0, matchedParam);
            }
            default:
            {
                return InnerMatch(args, context, 0);
            }
        }
    }

    private SignatureMatchResult InnerMatch(List<LuaExprSyntax> args, SearchContext context, int skipParam,
        int matchedParam = 0)
    {
        var matchResult = new SignatureMatchResult
        {
            MatchType = SignatureMatchType.ParamPartialMatch,
            MatchCount = matchedParam
        };

        if (skipParam >= Parameters.Count || args.Count == 0)
        {
            return matchResult;
        }

        var firstType = Parameters[skipParam].Info.DeclarationType ?? Builtin.Any;
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

        var paramCount = Parameters.Count - skipParam;
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

    public LuaSignature InstantiateSignature(
        LuaCallExprSyntax callExpr,
        List<LuaExprSyntax> args,
        HashSet<string> genericParameterNames,
        bool colonDefine,
        SearchContext context)
    {
        var colonCall = false;
        var genericParameterMap = new Dictionary<string, LuaType>();
        if (callExpr.PrefixExpr is LuaIndexExprSyntax indexExpr)
        {
            colonCall = indexExpr.IsColonIndex;
        }

        switch ((colonCall, colonDefine))
        {
            case (true, false):
            {
                return InnerInstantiate(callExpr, args, genericParameterNames, genericParameterMap, 1, 0, context);
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
                    return this;
                }

                var matchedParam = 0;
                var firstArgType = context.Infer(args[0]);
                if (firstArgType.SubTypeOf(selfType, context))
                {
                    matchedParam++;
                }

                return InnerInstantiate(callExpr, args, genericParameterNames, genericParameterMap, 0, matchedParam, context);
            }
            default:
            {
                return InnerInstantiate(callExpr, args, genericParameterNames, genericParameterMap, 0, 0, context);
            }
        }
    }

    private LuaSignature InnerInstantiate(
        LuaCallExprSyntax callExpr,
        List<LuaExprSyntax> args,
        HashSet<string> genericParameters,
        Dictionary<string, LuaType> genericParameterMap,
        int skipParam,
        int matchedParam,
        SearchContext context)
    {
        if (skipParam > Parameters.Count || args.Count == 0)
        {
            return this;
        }

        var newParameters = new List<LuaDeclaration>();
        if (skipParam == 1)
        {
            if (args.Count > 0 && callExpr.PrefixExpr is LuaIndexExprSyntax { PrefixExpr: { } callSelf })
            {
                var prefixType = context.Infer(callSelf);
                var parameterType = Parameters[0].Info.DeclarationType ?? Builtin.Any;
                GenericInfer.InferInstantiateByType(parameterType, prefixType, genericParameters, genericParameterMap,
                    context);
            }

            newParameters.Add(Parameters[0].Instantiate(genericParameterMap));
        }

        var paramStart = skipParam;
        var argStart = matchedParam;

        for (var i = 0;
             i + paramStart < Parameters.Count
             && i + argStart < args.Count
             && genericParameterMap.Count < genericParameters.Count;
             i++)
        {
            var parameter = Parameters[i + paramStart];
            if (parameter.Info is ParamInfo { IsVararg: true, DeclarationType: LuaExpandType expandType })
            {
                var varargs = args[(i + argStart)..];
                GenericInfer.InferInstantiateByExpandTypeAndExprs(expandType, varargs, genericParameters,
                    genericParameterMap, context);
            }
            else
            {
                var arg = args[i + argStart];
                var parameterType = parameter.Info.DeclarationType ?? Builtin.Any;
                GenericInfer.InferInstantiateByExpr(parameterType, arg, genericParameters, genericParameterMap,
                    context);
            }
        }

        var newReturnType = ReturnType.Instantiate(genericParameterMap);
        foreach (var parameter in Parameters)
        {
            newParameters.Add(parameter.Instantiate(genericParameterMap));
        }

        return new LuaSignature(newReturnType, newParameters);
    }

    public bool Equals(LuaSignature? other)
    {
        if (ReferenceEquals(this, other))
        {
            return true;
        }

        if (other is not null)
        {
            return ReturnType.Equals(other.ReturnType) && Parameters.SequenceEqual(other.Parameters);
        }

        return false;
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as LuaSignature);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Parameters);
    }

    public LuaSignature Instantiate(Dictionary<string, LuaType> genericReplace)
    {
        var newReturnType = ReturnType.Instantiate(genericReplace);
        var newParameters = Parameters
            .Select(parameter => parameter.Instantiate(genericReplace))
            .ToList();
        return new LuaSignature(newReturnType, newParameters);
    }
}

public class LuaMethodType(LuaSignature mainSignature, List<LuaSignature>? overloads, bool colonDefine)
    : LuaType(TypeKind.Method), IEquatable<LuaMethodType>
{
    public LuaSignature MainSignature { get; } = mainSignature;

    public List<LuaSignature>? Overloads { get; } = overloads;

    public bool ColonDefine { get; } = colonDefine;

    public LuaMethodType(LuaType returnType, List<LuaDeclaration> parameters, bool colonDefine)
        : this(new LuaSignature(returnType, parameters), null, colonDefine)
    {
    }

    public virtual LuaSignature FindPerfectMatchSignature(
        LuaCallExprSyntax callExpr,
        List<LuaExprSyntax> args,
        SearchContext context)
    {
        if (Overloads is null)
        {
            return MainSignature;
        }

        var maxMatch = MainSignature.Match(callExpr, args, context, ColonDefine);
        LuaSignature? perfectMatch = MainSignature;
        foreach (var signature in Overloads)
        {
            var match = signature.Match(callExpr, args, context, ColonDefine);
            if (match > maxMatch)
            {
                maxMatch = match;
                perfectMatch = signature;
            }
        }

        return perfectMatch;
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as LuaMethodType);
    }

    public override bool Equals(LuaType? other)
    {
        return Equals(other as LuaMethodType);
    }

    public bool Equals(LuaMethodType? other)
    {
        if (ReferenceEquals(this, other))
        {
            return true;
        }

        if (other is not null)
        {
            return MainSignature.Equals(other.MainSignature) && ColonDefine == other.ColonDefine;
        }

        return base.Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(base.GetHashCode(), MainSignature, ColonDefine);
    }

    public override LuaType Instantiate(Dictionary<string, LuaType> genericReplace)
    {
        var newMainSignature = MainSignature.Instantiate(genericReplace);
        var newOverloads = Overloads?.Select(signature => signature.Instantiate(genericReplace)).ToList();
        return new LuaMethodType(newMainSignature, newOverloads, ColonDefine);
    }
}

public class LuaGenericMethodType : LuaMethodType
{
    public List<LuaDeclaration> GenericParameterDeclarations { get; }

    public HashSet<string> GenericParameterNames { get; }

    public LuaGenericMethodType(
        List<LuaDeclaration> genericParameterDeclarations,
        LuaSignature mainSignature,
        List<LuaSignature>? overloads,
        bool colonDefine) : base(mainSignature, overloads, colonDefine)
    {
        GenericParameterDeclarations = genericParameterDeclarations;
        GenericParameterNames = genericParameterDeclarations.Select(declaration => declaration.Name).ToHashSet();
    }

    public List<LuaSignature> GetInstantiatedSignatures(
        LuaCallExprSyntax callExpr,
        List<LuaExprSyntax> args,
        SearchContext context)
    {
        var signatures = new List<LuaSignature>
            { MainSignature.InstantiateSignature(callExpr, args, GenericParameterNames, ColonDefine, context) };

        if (Overloads is not null)
        {
            signatures.AddRange(Overloads.Select(signature =>
                signature.InstantiateSignature(callExpr, args, GenericParameterNames, ColonDefine, context)));
        }

        return signatures;
    }

    public override LuaSignature FindPerfectMatchSignature(LuaCallExprSyntax callExpr, List<LuaExprSyntax> args,
        SearchContext context)
    {
        var signatures = new List<LuaSignature>();

        signatures.Add(MainSignature.InstantiateSignature(callExpr, args, GenericParameterNames, ColonDefine, context));

        if (Overloads is not null)
        {
            signatures.AddRange(Overloads.Select(signature =>
                signature.InstantiateSignature(callExpr, args, GenericParameterNames, ColonDefine, context)));
        }

        var maxMatch = SignatureMatchResult.NotMatch;
        LuaSignature perfectMatch = signatures.First();
        foreach (var signature in signatures)
        {
            var match = signature.Match(callExpr, args, context, ColonDefine);
            if (match > maxMatch)
            {
                maxMatch = match;
                perfectMatch = signature;
            }
        }

        return perfectMatch;
    }
}
