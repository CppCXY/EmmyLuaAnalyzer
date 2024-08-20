using EmmyLua.CodeAnalysis.Compilation.Infer;
using EmmyLua.CodeAnalysis.Compilation.Search;
using EmmyLua.CodeAnalysis.Compilation.Symbol;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace EmmyLua.CodeAnalysis.Type.Types;


public class LuaMultiReturnType : LuaType
{
    private List<LuaType>? RetTypes { get; }

    private LuaType? BaseType { get; }

    public LuaMultiReturnType(LuaType baseType)
    {
        BaseType = baseType;
    }

    public LuaMultiReturnType(List<LuaType> retTypes)
    {
        RetTypes = retTypes;
    }

    public LuaType GetElementType(int id)
    {
        if (RetTypes?.Count > id)
        {
            return RetTypes[id];
        }

        return BaseType ?? Builtin.Nil;
    }

    public int GetElementCount()
    {
        return RetTypes?.Count ?? 0;
    }

    public override LuaType Instantiate(TypeSubstitution substitution)
    {
        if (RetTypes is not null)
        {
            var returnTypes = new List<LuaType>();
            foreach (var retType in RetTypes)
            {
                var substituteType = retType.Instantiate(substitution);
                if (substituteType is LuaMultiReturnType multiReturnType)
                {
                    if (multiReturnType.RetTypes is { } retTypes)
                    {
                        returnTypes.AddRange(retTypes);
                    }
                    else if (multiReturnType.BaseType is { } baseType)
                    {
                        returnTypes.Add(baseType);
                    }
                }
                else
                {
                    returnTypes.Add(substituteType);
                }
            }

            return new LuaMultiReturnType(returnTypes);
        }
        else
        {
            return new LuaMultiReturnType(BaseType!.Instantiate(substitution));
        }
    }
}

public class LuaSignature(LuaType returnType, List<LuaSymbol> parameters)
{
    public LuaType ReturnType { get; set; } = returnType;

    public List<LuaSymbol> Parameters { get; } = parameters;

    public LuaSignature Instantiate(TypeSubstitution substitution)
    {
        var newReturnType = ReturnType.Instantiate(substitution);
        var newParameters = Parameters
            .Select(parameter => parameter.Instantiate(substitution))
            .ToList();
        return new LuaSignature(newReturnType, newParameters);
    }
}

public class LuaMethodType(LuaSignature mainSignature, List<LuaSignature>? overloads, bool colonDefine)
    : LuaType
{
    public LuaSignature MainSignature { get; } = mainSignature;

    public List<LuaSignature>? Overloads { get; } = overloads;

    public bool ColonDefine { get; } = colonDefine;

    public LuaMethodType(LuaType returnType, List<LuaSymbol> parameters, bool colonDefine)
        : this(new LuaSignature(returnType, parameters), null, colonDefine)
    {
    }

    public override LuaType Instantiate(TypeSubstitution substitution)
    {
        var newMainSignature = MainSignature.Instantiate(substitution);
        var newOverloads = Overloads?.Select(signature => signature.Instantiate(substitution)).ToList();
        return new LuaMethodType(newMainSignature, newOverloads, ColonDefine);
    }
}

public class LuaGenericMethodType : LuaMethodType
{
    public List<LuaTypeTemplate> GenericParamDecls { get; }

    public Dictionary<string, LuaTypeTemplate> GenericParams { get; }

    public LuaGenericMethodType(
        List<LuaTypeTemplate> genericParamDecls,
        LuaSignature mainSignature,
        List<LuaSignature>? overloads,
        bool colonDefine) : base(mainSignature, overloads, colonDefine)
    {
        GenericParamDecls = genericParamDecls;
        GenericParams = new Dictionary<string, LuaTypeTemplate>();
        foreach (var decl in GenericParamDecls)
        {
            GenericParams[decl.Name] = decl;
        }
    }

    public List<LuaSignature> GetInstantiatedSignatures(
        LuaCallExprSyntax callExpr,
        List<LuaExprSyntax> args,
        SearchContext context)
    {
        var signatures = new List<LuaSignature>
            { MethodInfer.InstantiateSignature(MainSignature, callExpr, args, GenericParams, ColonDefine, context) };

        if (Overloads is not null)
        {
            signatures.AddRange(Overloads.Select(signature =>
                MethodInfer.InstantiateSignature(signature, callExpr, args, GenericParams, ColonDefine, context)));
        }

        return signatures;
    }
}
