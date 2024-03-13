using EmmyLua.CodeAnalysis.Compilation.Infer;
using EmmyLua.CodeAnalysis.Compilation.Symbol;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace EmmyLua.CodeAnalysis.Compilation.Type;

public class LuaMultiReturnType(List<LuaType> retTypes) : LuaType(TypeKind.Return), IEquatable<LuaMultiReturnType>
{
    public List<LuaType> RetTypes { get; } = retTypes;

    public override bool Equals(object? obj)
    {
        return Equals(obj as LuaMultiReturnType);
    }

    public bool Equals(LuaMultiReturnType? other)
    {
        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return other is not null ? RetTypes.SequenceEqual(other.RetTypes) : base.Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(base.GetHashCode(), RetTypes);
    }
}

public enum SignatureMatchType
{
    DoNotMatch,
    ParamPartialMatch,
    ParamCountMatch,
    EmitterStyle
}

public struct SignatureMatchResult
{
    public static readonly SignatureMatchResult NotMatch = new SignatureMatchResult()
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

public class LuaSignature(LuaType returnType, List<ParameterDeclaration> parameters) : IEquatable<LuaSignature>
{
    public LuaType ReturnType { get; set; } = returnType;

    public List<ParameterDeclaration> Parameters { get; } = parameters;

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
        var matchResult = new SignatureMatchResult()
        {
            MatchType = SignatureMatchType.ParamPartialMatch,
            MatchCount = matchedParam
        };

        if (skipParam > Parameters.Count || args.Count == 0)
        {
            return matchResult;
        }

        var firstType = Parameters[skipParam].DeclarationType ?? Builtin.Any;
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
        if (callExpr.PrefixExpr is LuaIndexExprSyntax indexExpr)
        {
            colonCall = indexExpr.IsColonIndex;
        }

        switch ((colonCall, colonDefine))
        {
            case (true, false):
            {
                return InnerInstantiate(args, genericParameterNames, 1, 0, context);
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

                return InnerInstantiate(args, genericParameterNames, 0, matchedParam, context);
            }
            default:
            {
                return InnerInstantiate(args, genericParameterNames, 0, 0, context);
            }
        }
    }

    private LuaSignature InnerInstantiate(
        List<LuaExprSyntax> args,
        HashSet<string> genericParameters,
        int skipParam,
        int matchedParam,
        SearchContext context)
    {
        if (skipParam > Parameters.Count || args.Count == 0)
        {
            return this;
        }

        var newParameters = new List<ParameterDeclaration>();
        if (skipParam > 0)
        {
            newParameters.AddRange(Parameters.Take(skipParam));
        }

        var genericParameterMap = new Dictionary<string, LuaType>();
        var paramStart = skipParam;
        var argStart = matchedParam;
        for (var i = 0;
             i + paramStart < Parameters.Count
             && i + argStart < args.Count
             && genericParameterMap.Count < genericParameters.Count;
             i++)
        {
            var parameter = Parameters[i + paramStart];
            var arg = args[i + argStart];
            var parameterType = parameter.DeclarationType ?? Builtin.Any;
            GenericInfer.InferInstantiateByExpr(parameterType, arg, genericParameters, genericParameterMap, context);
        }

        // for (var i = skipParam; i < Parameters.Count; i++)
        // {
        //     var parameter = Parameters[i];
        //     var newType = parameter.DeclarationType?.Instantiate(genericParameters) ?? Builtin.Any;
        //     newParameters.Add(new ParameterDeclaration(parameter.Name, newType));
        // }
        //
        // return new LuaSignature(newReturnType, newParameters);
        throw new NotImplementedException();
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
}

public class LuaMethodType(LuaSignature mainSignature, List<LuaSignature>? overloads, bool colonDefine)
    : LuaType(TypeKind.Method), IEquatable<LuaMethodType>
{
    public LuaSignature MainSignature { get; } = mainSignature;

    public List<LuaSignature>? Overloads { get; } = overloads;

    public bool ColonDefine { get; } = colonDefine;

    public LuaMethodType(LuaType returnType, List<ParameterDeclaration> parameters, bool colonDefine)
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
}

public class LuaGenericMethodType : LuaMethodType
{
    public List<GenericParameterDeclaration> GenericParameterDeclarations { get; }

    public HashSet<string> GenericParameterNames { get; }

    public LuaGenericMethodType(
        List<GenericParameterDeclaration> genericParameterDeclarations,
        LuaSignature mainSignature,
        List<LuaSignature>? overloads,
        bool colonDefine) : base(mainSignature, overloads, colonDefine)
    {
        GenericParameterDeclarations = genericParameterDeclarations;
        GenericParameterNames = genericParameterDeclarations.Select(declaration => declaration.Name).ToHashSet();
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
        LuaSignature? perfectMatch = null;
        foreach (var signature in signatures)
        {
            var match = signature.Match(callExpr, args, context, ColonDefine);
            if (match > maxMatch)
            {
                maxMatch = match;
                perfectMatch = signature;
            }
        }

        return perfectMatch!;
    }
}
