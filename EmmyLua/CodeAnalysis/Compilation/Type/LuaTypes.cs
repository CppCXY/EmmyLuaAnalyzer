using EmmyLua.CodeAnalysis.Common;
using EmmyLua.CodeAnalysis.Compilation.Declaration;
using EmmyLua.CodeAnalysis.Compilation.Infer;
using EmmyLua.CodeAnalysis.Compilation.Search;
using EmmyLua.CodeAnalysis.Syntax.Node;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace EmmyLua.CodeAnalysis.Compilation.Type;

public class LuaType(LuaTypeAttribute attribute) : IEquatable<LuaType>
{
    public LuaTypeAttribute Attribute { get; } = attribute;

    public bool HasMember => Attribute.HasFlag(LuaTypeAttribute.HasMember);

    public bool CanIndex => Attribute.HasFlag(LuaTypeAttribute.CanIndex);

    public bool CanCall => Attribute.HasFlag(LuaTypeAttribute.CanCall);

    public override bool Equals(object? obj)
    {
        return Equals(obj as LuaType);
    }

    public virtual bool Equals(LuaType? other)
    {
        return ReferenceEquals(this, other);
    }

    public override int GetHashCode()
    {
        return (int)Attribute;
    }

    public bool SubTypeOf(LuaType? other, SearchContext context)
    {
        return other is not null && context.IsSubTypeOf(this, other);
    }

    public virtual LuaType Instantiate(TypeSubstitution substitution)
    {
        return this;
    }

    public virtual LuaType UnwrapType(SearchContext context)
    {
        return this;
    }
}

public class LuaNamedType(
    string name,
    LuaTypeAttribute attribute = LuaTypeAttribute.HasMember | LuaTypeAttribute.CanCall | LuaTypeAttribute.CanIndex)
    : LuaType(attribute), IEquatable<LuaNamedType>
{
    public static LuaNamedType Create(string name, LuaTypeAttribute attribute = LuaTypeAttribute.HasMember | LuaTypeAttribute.CanCall | LuaTypeAttribute.CanIndex)
    {
        var buildType = Builtin.FromName(name);
        if (buildType is not null)
        {
            return buildType;
        }

        return new LuaNamedType(name, attribute);
    }

    public string Name { get; } = name;

    public NamedTypeKind GetTypeKind(SearchContext context)
    {
        return context.Compilation.Db.QueryNamedTypeKind(Name);
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as LuaNamedType);
    }

    public override bool Equals(LuaType? other)
    {
        return Equals(other as LuaNamedType);
    }

    public bool Equals(LuaNamedType? other)
    {
        return string.Equals(Name, other?.Name, StringComparison.CurrentCulture);
    }

    public override int GetHashCode()
    {
        return Name.GetHashCode();
    }

    public override LuaType Instantiate(TypeSubstitution substitution)
    {
        return substitution.Substitute(Name, this);
    }
}

public class LuaTemplateType(string prefixName, string templateName)
    : LuaType(LuaTypeAttribute.None), IEquatable<LuaTemplateType>
{
    public string TemplateName { get; } = templateName;

    public string PrefixName { get; } = prefixName;

    public bool Equals(LuaTemplateType? other)
    {
        return TemplateName == other?.TemplateName && PrefixName == other.PrefixName;
    }
}

public class LuaUnionType(IEnumerable<LuaType> unionTypes)
    : LuaType(LuaTypeAttribute.CanCall | LuaTypeAttribute.CanIndex | LuaTypeAttribute.HasMember),
        IEquatable<LuaUnionType>
{
    public HashSet<LuaType> UnionTypes { get; } = [..unionTypes];

    public override bool Equals(object? obj)
    {
        return Equals(obj as LuaUnionType);
    }

    public override bool Equals(LuaType? other)
    {
        return Equals(other as LuaUnionType);
    }

    public bool Equals(LuaUnionType? other)
    {
        return base.Equals(other) && UnionTypes.SetEquals(other.UnionTypes);
    }

    public override int GetHashCode()
    {
        return UnionTypes.GetHashCode();
    }

    public override LuaType Instantiate(TypeSubstitution substitution)
    {
        var newUnionTypes = UnionTypes.Select(t => t.Instantiate(substitution));
        return new LuaUnionType(newUnionTypes);
    }
}

public class LuaAggregateType(IEnumerable<IDeclaration> declarations)
    : LuaType(LuaTypeAttribute.None), IEquatable<LuaAggregateType>
{
    public List<IDeclaration> Declarations { get; } = declarations.ToList();

    public override bool Equals(object? obj)
    {
        return Equals(obj as LuaAggregateType);
    }

    public override bool Equals(LuaType? other)
    {
        return Equals(other as LuaAggregateType);
    }

    public bool Equals(LuaAggregateType? other)
    {
        return base.Equals(other) && Declarations
            .Select(it => it.Type)
            .SequenceEqual(other.Declarations.Select(it => it.Type));
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Declarations);
    }

    public override LuaType Instantiate(TypeSubstitution substitution)
    {
        var newAggregateTypes = Declarations.Select(t => t.Instantiate(substitution));
        return new LuaAggregateType(newAggregateTypes);
    }
}

public class LuaTupleType(List<IDeclaration> tupleDeclaration)
    : LuaType(LuaTypeAttribute.HasMember | LuaTypeAttribute.CanIndex), IEquatable<LuaTupleType>
{
    public List<IDeclaration> TupleDeclaration { get; } = tupleDeclaration;

    public override bool Equals(object? obj)
    {
        return Equals(obj as LuaTupleType);
    }

    public override bool Equals(LuaType? other)
    {
        return Equals(other as LuaTupleType);
    }

    public bool Equals(LuaTupleType? other)
    {
        if (ReferenceEquals(this, other)) return true;
        if (other is not null)
        {
            return TupleDeclaration.Select(it => it.Type)
                .SequenceEqual(other.TupleDeclaration.Select(it => it.Type));
        }

        return false;
    }

    public override int GetHashCode()
    {
        return TupleDeclaration.GetHashCode();
    }

    public override LuaType Instantiate(TypeSubstitution substitution)
    {
        var newTupleTypes = TupleDeclaration
            .Select(t => t.Instantiate(substitution))
            .ToList();
        if (newTupleTypes.Count != 0 && newTupleTypes[^1].Type is LuaMultiReturnType multiReturnType)
        {
            var lastMember = newTupleTypes[^1];
            newTupleTypes.RemoveAt(newTupleTypes.Count - 1);
            if (lastMember is LuaDeclaration { Info: TupleMemberInfo info } lastMember2)
            {
                for (var i = 0; i < multiReturnType.GetElementCount(); i++)
                {
                    newTupleTypes.Add(lastMember2.WithInfo(
                        info with
                        {
                            Index = info.Index + i,
                            DeclarationType = multiReturnType.GetElementType(i),
                        }));
                }
            }
        }

        return new LuaTupleType(newTupleTypes);
    }
}

public class LuaArrayType(LuaType baseType)
    : LuaType(LuaTypeAttribute.CanIndex), IEquatable<LuaArrayType>
{
    public LuaType BaseType { get; } = baseType;

    public override bool Equals(object? obj)
    {
        return Equals(obj as LuaArrayType);
    }

    public override bool Equals(LuaType? other)
    {
        return Equals(other as LuaArrayType);
    }

    public bool Equals(LuaArrayType? other)
    {
        return BaseType.Equals(other?.BaseType);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(base.GetHashCode(), BaseType);
    }

    public override LuaType Instantiate(TypeSubstitution substitution)
    {
        var newBaseType = BaseType.Instantiate(substitution);
        return new LuaArrayType(newBaseType);
    }
}

public class LuaGenericType(string baseName, List<LuaType> genericArgs)
    : LuaNamedType(baseName), IEquatable<LuaGenericType>
{
    public List<LuaType> GenericArgs { get; } = genericArgs;

    public override bool Equals(object? obj)
    {
        return Equals(obj as LuaGenericType);
    }

    public override bool Equals(LuaType? other)
    {
        return Equals(other as LuaGenericType);
    }

    public bool Equals(LuaGenericType? other)
    {
        return base.Equals(other) && GenericArgs.Equals(other.GenericArgs);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(base.GetHashCode(), GenericArgs);
    }

    public override LuaType Instantiate(TypeSubstitution substitution)
    {
        var newName = Name;
        if (substitution.Substitute(Name) is LuaNamedType namedType)
        {
            newName = namedType.Name;
        }

        var newGenericArgs = GenericArgs.Select(t => t.Instantiate(substitution)).ToList();
        return new LuaGenericType(newName, newGenericArgs);
    }
}

public class LuaStringLiteralType(string content)
    : LuaType(LuaTypeAttribute.None), IEquatable<LuaStringLiteralType>
{
    public string Content { get; } = content;

    public override bool Equals(object? obj)
    {
        return Equals(obj as LuaStringLiteralType);
    }

    public override bool Equals(LuaType? other)
    {
        return Equals(other as LuaStringLiteralType);
    }

    public bool Equals(LuaStringLiteralType? other)
    {
        return Content == other?.Content;
    }

    public override int GetHashCode()
    {
        return Content.GetHashCode();
    }
}

public class LuaIntegerLiteralType(long value)
    : LuaType(LuaTypeAttribute.None), IEquatable<LuaIntegerLiteralType>
{
    public long Value { get; } = value;

    public override bool Equals(object? obj)
    {
        return Equals(obj as LuaIntegerLiteralType);
    }

    public override bool Equals(LuaType? other)
    {
        return Equals(other as LuaIntegerLiteralType);
    }

    public bool Equals(LuaIntegerLiteralType? other)
    {
        return Value == other?.Value;
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }
}

public class LuaTableLiteralType(LuaTableExprSyntax tableExpr)
    : LuaType(LuaTypeAttribute.CanCall | LuaTypeAttribute.CanIndex | LuaTypeAttribute.HasMember),
        IEquatable<LuaTableLiteralType>
{
    public LuaElementPtr<LuaTableExprSyntax> TableExprPtr { get; } = new(tableExpr);

    public override bool Equals(object? obj)
    {
        return Equals(obj as LuaTableLiteralType);
    }

    public override bool Equals(LuaType? other)
    {
        return Equals(other as LuaTableLiteralType);
    }

    public bool Equals(LuaTableLiteralType? other)
    {
        return TableExprPtr.Equals(other?.TableExprPtr);
    }

    public override int GetHashCode()
    {
        return TableExprPtr.GetHashCode();
    }
}

public class LuaDocTableType(LuaDocTableTypeSyntax tableType)
    : LuaType(LuaTypeAttribute.CanIndex | LuaTypeAttribute.HasMember),
        IEquatable<LuaDocTableType>
{
    public LuaElementPtr<LuaDocTableTypeSyntax> DocTablePtr { get; } = new(tableType);

    public override bool Equals(object? obj)
    {
        return Equals(obj as LuaDocTableType);
    }

    public override bool Equals(LuaType? other)
    {
        return Equals(other as LuaDocTableType);
    }

    public bool Equals(LuaDocTableType? other)
    {
        return DocTablePtr.Equals(other?.DocTablePtr);
    }

    public override int GetHashCode()
    {
        return DocTablePtr.GetHashCode();
    }
}

public class LuaVariadicType(LuaType baseType)
    : LuaType(LuaTypeAttribute.None), IEquatable<LuaVariadicType>
{
    public LuaType BaseType { get; } = baseType;

    public override bool Equals(object? obj)
    {
        return Equals(obj as LuaVariadicType);
    }

    public override bool Equals(LuaType? other)
    {
        return Equals(other as LuaVariadicType);
    }

    public bool Equals(LuaVariadicType? other)
    {
        if (ReferenceEquals(this, other)) return true;
        return base.Equals(other) && BaseType.Equals(other.BaseType);
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }

    public override LuaType Instantiate(TypeSubstitution substitution)
    {
        var newBaseType = BaseType.Instantiate(substitution);
        return new LuaVariadicType(newBaseType);
    }
}

public class LuaExpandType(string baseName)
    : LuaType(LuaTypeAttribute.None), IEquatable<LuaExpandType>
{
    public string Name { get; } = baseName;

    public override bool Equals(object? obj)
    {
        return Equals(obj as LuaExpandType);
    }

    public override bool Equals(LuaType? other)
    {
        return Equals(other as LuaExpandType);
    }

    public bool Equals(LuaExpandType? other)
    {
        if (ReferenceEquals(this, other)) return true;
        return base.Equals(other);
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }

    public override LuaType Instantiate(TypeSubstitution substitution)
    {
        return substitution.Substitute(Name, this);
    }
}

public class LuaMultiReturnType : LuaType, IEquatable<LuaMultiReturnType>
{
    private List<LuaType>? RetTypes { get; }

    private LuaType? BaseType { get; }

    public LuaMultiReturnType(LuaType baseType)
        : base(LuaTypeAttribute.None)
    {
        BaseType = baseType;
    }

    public LuaMultiReturnType(List<LuaType> retTypes)
        : base(LuaTypeAttribute.None)
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

    public override bool Equals(object? obj)
    {
        return Equals(obj as LuaMultiReturnType);
    }

    public override bool Equals(LuaType? other)
    {
        return Equals(other as LuaMultiReturnType);
    }

    public bool Equals(LuaMultiReturnType? other)
    {
        if (ReferenceEquals(this, other))
        {
            return true;
        }

        if (other is null)
        {
            return false;
        }

        if (RetTypes is not null && other.RetTypes is not null)
        {
            return RetTypes.SequenceEqual(other.RetTypes);
        }
        else if (BaseType is not null && other.BaseType is not null)
        {
            return BaseType.Equals(other.BaseType);
        }

        return false;
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

    public override int GetHashCode()
    {
        return HashCode.Combine(base.GetHashCode(), RetTypes);
    }
}

public class LuaSignature(LuaType returnType, List<IDeclaration> parameters) : IEquatable<LuaSignature>
{
    public LuaType ReturnType { get; set; } = returnType;

    public List<IDeclaration> Parameters { get; } = parameters;

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
    : LuaType(LuaTypeAttribute.CanCall), IEquatable<LuaMethodType>
{
    public LuaSignature MainSignature { get; } = mainSignature;

    public List<LuaSignature>? Overloads { get; } = overloads;

    public bool ColonDefine { get; } = colonDefine;

    public LuaMethodType(LuaType returnType, List<IDeclaration> parameters, bool colonDefine)
        : this(new LuaSignature(returnType, parameters), null, colonDefine)
    {
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

    public override LuaType Instantiate(TypeSubstitution substitution)
    {
        var newMainSignature = MainSignature.Instantiate(substitution);
        var newOverloads = Overloads?.Select(signature => signature.Instantiate(substitution)).ToList();
        return new LuaMethodType(newMainSignature, newOverloads, ColonDefine);
    }
}

public class LuaGenericMethodType : LuaMethodType
{
    public List<LuaDeclaration> GenericParamDecls { get; }

    public Dictionary<string, LuaType> GenericParams { get; }

    public LuaGenericMethodType(
        List<LuaDeclaration> genericParamDecls,
        LuaSignature mainSignature,
        List<LuaSignature>? overloads,
        bool colonDefine) : base(mainSignature, overloads, colonDefine)
    {
        GenericParamDecls = genericParamDecls;
        GenericParams = new Dictionary<string, LuaType>();
        foreach (var decl in GenericParamDecls)
        {
            GenericParams[decl.Name] = decl.Type;
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

public class LuaVariableRefType(SyntaxElementId id)
    : LuaType(LuaTypeAttribute.HasMember | LuaTypeAttribute.CanIndex | LuaTypeAttribute.CanCall),
        IEquatable<LuaVariableRefType>
{
    public SyntaxElementId Id { get; } = id;

    public override bool Equals(object? obj)
    {
        return Equals(obj as LuaVariableRefType);
    }

    public bool Equals(LuaVariableRefType? other)
    {
        return Id == other?.Id;
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }

    public override LuaType UnwrapType(SearchContext context)
    {
        return context.Compilation.Db.QueryTypeFromId(Id) ?? this;
    }
}

public class GlobalNameType(string name)
    : LuaType(LuaTypeAttribute.CanCall | LuaTypeAttribute.CanIndex | LuaTypeAttribute.HasMember),
        IEquatable<GlobalNameType>
{
    public string Name { get; } = name;

    public override bool Equals(object? obj)
    {
        return Equals(obj as GlobalNameType);
    }

    public bool Equals(GlobalNameType? other)
    {
        return Name == other?.Name;
    }

    public override int GetHashCode()
    {
        return Name.GetHashCode();
    }

    public override LuaType UnwrapType(SearchContext context)
    {
        return context.Compilation.Db.QueryRelatedGlobalType(Name) ?? this;
    }
}
