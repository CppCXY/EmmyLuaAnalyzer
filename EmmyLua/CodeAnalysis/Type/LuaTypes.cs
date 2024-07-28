using EmmyLua.CodeAnalysis.Compilation.Infer;
using EmmyLua.CodeAnalysis.Compilation.Search;
using EmmyLua.CodeAnalysis.Compilation.Symbol;
using EmmyLua.CodeAnalysis.Document;
using EmmyLua.CodeAnalysis.Syntax.Node;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace EmmyLua.CodeAnalysis.Type;

public class LuaType
{
    public bool SubTypeOf(LuaType? other, SearchContext context)
    {
        return other is not null && context.IsSubTypeOf(this, other);
    }

    public bool IsSameType(LuaType? other, SearchContext context)
    {
        return other is not null && context.IsSameType(this, other);
    }

    public virtual LuaType Instantiate(TypeSubstitution substitution)
    {
        return this;
    }
}

public class LuaNamedType(LuaDocumentId documentId, string name)
    : LuaType
{
    public LuaDocumentId DocumentId { get; } = documentId;

    public string Name { get; } = name;

    public override LuaType Instantiate(TypeSubstitution substitution)
    {
        return substitution.Substitute(Name, this);
    }
}

public class LuaTemplateType(string prefixName, string templateName)
    : LuaType
{
    public string TemplateName { get; } = templateName;

    public string PrefixName { get; } = prefixName;
}

public class LuaUnionType(IEnumerable<LuaType> unionTypes)
    : LuaType
{
    public HashSet<LuaType> UnionTypes { get; } = [..unionTypes];

    public override LuaType Instantiate(TypeSubstitution substitution)
    {
        var newUnionTypes = UnionTypes.Select(t => t.Instantiate(substitution));
        return new LuaUnionType(newUnionTypes);
    }
}

public class LuaAggregateType(IEnumerable<LuaSymbol> declarations)
    : LuaType
{
    public List<LuaSymbol> Declarations { get; } = declarations.ToList();

    public override LuaType Instantiate(TypeSubstitution substitution)
    {
        var newAggregateTypes = Declarations.Select(t => t.Instantiate(substitution));
        return new LuaAggregateType(newAggregateTypes);
    }
}

public class LuaTupleType(List<LuaSymbol> tupleDeclaration)
    : LuaType
{
    public List<LuaSymbol> TupleDeclaration { get; } = tupleDeclaration;

    public override LuaType Instantiate(TypeSubstitution substitution)
    {
        var newTupleTypes = TupleDeclaration
            .Select(t => t.Instantiate(substitution))
            .ToList();
        if (newTupleTypes.Count != 0 && newTupleTypes[^1].Type is LuaMultiReturnType multiReturnType)
        {
            var lastMember = newTupleTypes[^1];
            newTupleTypes.RemoveAt(newTupleTypes.Count - 1);
            if (lastMember is { Info: TupleMemberInfo info } lastMember2)
            {
                for (var i = 0; i < multiReturnType.GetElementCount(); i++)
                {
                    newTupleTypes.Add(lastMember2.WithInfo(info with
                        {
                            Index = info.Index + i,
                        }).WithType(multiReturnType.GetElementType(i))
                    );
                }
            }
        }

        return new LuaTupleType(newTupleTypes);
    }
}

public class LuaArrayType(LuaType baseType)
    : LuaType
{
    public LuaType BaseType { get; } = baseType;

    public override LuaType Instantiate(TypeSubstitution substitution)
    {
        var newBaseType = BaseType.Instantiate(substitution);
        return new LuaArrayType(newBaseType);
    }
}

public class LuaGenericType(LuaDocumentId documentId, string baseName, List<LuaType> genericArgs)
    : LuaNamedType(documentId, baseName)
{
    public List<LuaType> GenericArgs { get; } = genericArgs;

    public override LuaType Instantiate(TypeSubstitution substitution)
    {
        var newName = Name;
        if (substitution.Substitute(Name) is LuaNamedType namedType)
        {
            newName = namedType.Name;
        }

        var newGenericArgs = GenericArgs.Select(t => t.Instantiate(substitution)).ToList();
        return new LuaGenericType(DocumentId, newName, newGenericArgs);
    }
}

public class LuaStringLiteralType(string content)
    : LuaType
{
    public string Content { get; } = content;
}

public class LuaIntegerLiteralType(long value)
    : LuaType
{
    public long Value { get; } = value;
}

public class LuaVariadicType(LuaType baseType)
    : LuaType
{
    public LuaType BaseType { get; } = baseType;

    public override LuaType Instantiate(TypeSubstitution substitution)
    {
        var newBaseType = BaseType.Instantiate(substitution);
        return new LuaVariadicType(newBaseType);
    }
}

public class LuaExpandType(string baseName)
    : LuaType
{
    public string Name { get; } = baseName;

    public override LuaType Instantiate(TypeSubstitution substitution)
    {
        return substitution.Substitute(Name, this);
    }
}

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
    public List<LuaSymbol> GenericParamDecls { get; }

    public Dictionary<string, LuaType> GenericParams { get; }

    public LuaGenericMethodType(
        List<LuaSymbol> genericParamDecls,
        LuaSignature mainSignature,
        List<LuaSignature>? overloads,
        bool colonDefine) : base(mainSignature, overloads, colonDefine)
    {
        GenericParamDecls = genericParamDecls;
        GenericParams = new Dictionary<string, LuaType>();
        foreach (var decl in GenericParamDecls)
        {
            if (decl.Type is not null)
            {
                GenericParams[decl.Name] = decl.Type;
            }
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

public class LuaElementType(SyntaxElementId id)
    : LuaType
{
    public SyntaxElementId Id { get; } = id;

    public LuaSyntaxElement? ToSyntaxElement(SearchContext context)
    {
        var document = context.Compilation.Project.GetDocument(Id.DocumentId);
        return document?.SyntaxTree.GetElement(Id.ElementId);
    }
}

public class GlobalNameType(string name)
    : LuaType
{
    public string Name { get; } = name;
}
