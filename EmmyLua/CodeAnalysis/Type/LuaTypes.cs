using EmmyLua.CodeAnalysis.Common;
using EmmyLua.CodeAnalysis.Compilation.Declaration;
using EmmyLua.CodeAnalysis.Compilation.Search;
using EmmyLua.CodeAnalysis.Syntax.Node;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace EmmyLua.CodeAnalysis.Type;

public class LuaType(TypeKind kind) : IEquatable<LuaType>
{
    public TypeKind Kind { get; } = kind;

    public override bool Equals(object? obj)
    {
        return Equals(obj as LuaType);
    }

    public virtual bool Equals(LuaType? other)
    {
        if (ReferenceEquals(this, other))
        {
            return true;
        }

        if (other is null)
        {
            return false;
        }

        return Kind == other.Kind;
    }

    public override int GetHashCode()
    {
        return (int) Kind;
    }

    public bool SubTypeOf(LuaType? other, SearchContext context)
    {
        return other is not null && context.IsSubTypeOf(this, other);
    }

    public virtual LuaType Instantiate(Dictionary<string, LuaType> genericReplace)
    {
        return this;
    }
}

public class LuaNamedType(string name, TypeKind kind = TypeKind.NamedType) : LuaType(kind), IEquatable<LuaNamedType>
{
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
        if (ReferenceEquals(this, other))
        {
            return true;
        }

        if (other is not null)
        {
            return Name == other.Name;
        }

        if (!base.Equals(other))
        {
            return false;
        }

        return string.Equals(Name, other.Name, StringComparison.CurrentCulture);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(base.GetHashCode(), Name);
    }

    public override LuaType Instantiate(Dictionary<string, LuaType> genericReplace)
    {
        return genericReplace.TryGetValue(Name, out var type) ? type : this;
    }
}

public class LuaTemplateType(string templateName): LuaType(TypeKind.Template), IEquatable<LuaTemplateType>
{
    public string TemplateName { get; } = templateName;

    public bool Equals(LuaTemplateType? other)
    {
        if (ReferenceEquals(this, other)) return true;
        return base.Equals(other) && TemplateName == other.TemplateName;
    }
}

public class LuaUnionType(IEnumerable<LuaType> unionTypes) : LuaType(TypeKind.Union), IEquatable<LuaUnionType>
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
        if (ReferenceEquals(this, other)) return true;
        return base.Equals(other) && UnionTypes.SetEquals(other.UnionTypes);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(base.GetHashCode(), UnionTypes);
    }

    public override LuaType Instantiate(Dictionary<string, LuaType> genericReplace)
    {
        var newUnionTypes = UnionTypes.Select(t => t.Instantiate(genericReplace));
        return new LuaUnionType(newUnionTypes);
    }
}

public class LuaAggregateType(IEnumerable<IDeclaration> declarations)
    : LuaType(TypeKind.Aggregate), IEquatable<LuaAggregateType>
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
        if (ReferenceEquals(this, other)) return true;
        return base.Equals(other) && Declarations
            .Select(it => it.Type)
            .SequenceEqual(other.Declarations.Select(it => it.Type));
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(base.GetHashCode(), Declarations);
    }

    public override LuaType Instantiate(Dictionary<string, LuaType> genericReplace)
    {
        var newAggregateTypes = Declarations.Select(t => t.Instantiate(genericReplace));
        return new LuaAggregateType(newAggregateTypes);
    }
}

public class LuaTupleType(List<IDeclaration> tupleDeclaration)
    : LuaType(TypeKind.Tuple), IEquatable<LuaTupleType>
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
        return HashCode.Combine(base.GetHashCode(), TupleDeclaration);
    }

    public override LuaType Instantiate(Dictionary<string, LuaType> genericReplace)
    {
        var newTupleTypes = TupleDeclaration
            .Select(t => t.Instantiate(genericReplace))
            .ToList();
        if (newTupleTypes.Count != 0 && newTupleTypes[^1].Type is LuaMultiReturnType multiReturnType)
        {
            var lastMember = newTupleTypes[^1];
            newTupleTypes.RemoveAt(newTupleTypes.Count - 1);
            if (lastMember is LuaDeclaration{ Info: TupleMemberInfo info} lastMember2)
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

public class LuaArrayType(LuaType baseType) : LuaType(TypeKind.Array), IEquatable<LuaArrayType>
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
        if (ReferenceEquals(this, other)) return true;
        return base.Equals(other) && BaseType.Equals(other.BaseType);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(base.GetHashCode(), BaseType);
    }

    public override LuaType Instantiate(Dictionary<string, LuaType> genericReplace)
    {
        var newBaseType = BaseType.Instantiate(genericReplace);
        return new LuaArrayType(newBaseType);
    }
}

public class LuaGenericType(string baseName, List<LuaType> genericArgs)
    : LuaNamedType(baseName, TypeKind.Generic), IEquatable<LuaGenericType>
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
        if (ReferenceEquals(this, other)) return true;
        return base.Equals(other) && GenericArgs.Equals(other.GenericArgs);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(base.GetHashCode(), GenericArgs);
    }

    public override LuaType Instantiate(Dictionary<string, LuaType> genericReplace)
    {
        var newName = Name;
        if (genericReplace.TryGetValue(Name, out var type))
        {
            if (type is LuaNamedType namedType)
            {
                newName = namedType.Name;
            }
        }

        var newGenericArgs = GenericArgs.Select(t => t.Instantiate(genericReplace)).ToList();
        return new LuaGenericType(newName, newGenericArgs);
    }
}

public class LuaStringLiteralType(string content) : LuaType(TypeKind.StringLiteral), IEquatable<LuaStringLiteralType>
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
        if (ReferenceEquals(this, other)) return true;
        return base.Equals(other) && Content == other.Content;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(base.GetHashCode(), Content);
    }

}

public class LuaIntegerLiteralType(long value) : LuaType(TypeKind.IntegerLiteral), IEquatable<LuaIntegerLiteralType>
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
        if (ReferenceEquals(this, other)) return true;
        return base.Equals(other) && Value == other.Value;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(base.GetHashCode(), Value);
    }

}

public class LuaTableLiteralType(LuaTableExprSyntax tableExpr)
    : LuaNamedType(tableExpr.UniqueString, TypeKind.TableLiteral), IEquatable<LuaTableLiteralType>
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
        if (ReferenceEquals(this, other)) return true;
        return base.Equals(other) && TableExprPtr.Equals(other.TableExprPtr);
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }
}

public class LuaDocTableType(LuaDocTableTypeSyntax tableType)
    : LuaNamedType(tableType.UniqueString, TypeKind.TableLiteral), IEquatable<LuaDocTableType>
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
        if (ReferenceEquals(this, other)) return true;
        return base.Equals(other) && DocTablePtr.Equals(other.DocTablePtr);
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }
}

public class LuaVariadicType(LuaType baseType) : LuaType(TypeKind.Variadic), IEquatable<LuaVariadicType>
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

    public override LuaType Instantiate(Dictionary<string, LuaType> genericReplace)
    {
        var newBaseType = BaseType.Instantiate(genericReplace);
        return new LuaVariadicType(newBaseType);
    }
}

public class LuaExpandType(string baseName) : LuaNamedType(baseName), IEquatable<LuaExpandType>
{
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

    public override LuaType Instantiate(Dictionary<string, LuaType> genericReplace)
    {
        if (genericReplace.TryGetValue(Name, out var type))
        {
            if (type is LuaMultiReturnType returnType)
            {
                return returnType;
            }
        }

        return this;
    }
}

public class LuaMultiReturnType : LuaType, IEquatable<LuaMultiReturnType>
{
    private List<LuaType>? RetTypes { get; }

    private LuaType? BaseType { get; }

    public LuaMultiReturnType(LuaType baseType)
        : base(TypeKind.Return)
    {
        BaseType = baseType;
    }

    public LuaMultiReturnType(List<LuaType> retTypes)
        : base(TypeKind.Return)
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

    public override LuaType Instantiate(Dictionary<string, LuaType> genericReplace)
    {
        if (RetTypes is not null)
        {
            var returnTypes = new List<LuaType>();
            foreach (var retType in RetTypes)
            {
                returnTypes.Add(retType.Instantiate(genericReplace));
            }

            return new LuaMultiReturnType(returnTypes);
        }
        else
        {
            return new LuaMultiReturnType(BaseType!.Instantiate(genericReplace));
        }
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(base.GetHashCode(), RetTypes);
    }
}
