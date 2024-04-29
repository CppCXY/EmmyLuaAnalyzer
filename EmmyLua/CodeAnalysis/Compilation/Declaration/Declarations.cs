using EmmyLua.CodeAnalysis.Compilation.Type;
using EmmyLua.CodeAnalysis.Syntax.Node;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace EmmyLua.CodeAnalysis.Compilation.Declaration;

public class DeclarationNode(int position)
{
    public DeclarationNode? Prev { get; set; }

    public DeclarationNode? Next { get; set; }

    public DeclarationNodeContainer? Parent { get; set; }

    public int Position { get; } = position;
}

public abstract class DeclarationNodeContainer(int position)
    : DeclarationNode(position)
{
    public List<DeclarationNode> Children { get; } = [];

    public void Add(DeclarationNode node)
    {
        node.Parent = this;

        // 如果Children为空，直接添加
        if (Children.Count == 0)
        {
            Children.Add(node);
            return;
        }

        // 如果Children的最后一个节点的位置小于等于node的位置，直接添加
        if (Children.Last().Position <= node.Position)
        {
            var last = Children.Last();
            node.Prev = last;
            last.Next = node;
            Children.Add(node);
        }
        else
        {
            var index = Children.FindIndex(n => n.Position > node.Position);
            // 否则，插入到找到的位置
            var nextNode = Children[index];
            var prevNode = nextNode.Prev;

            node.Next = nextNode;
            node.Prev = prevNode;

            if (prevNode != null)
            {
                prevNode.Next = node;
            }

            nextNode.Prev = node;

            Children.Insert(index, node);
        }
    }

    public DeclarationNode? FirstChild => Children.FirstOrDefault();

    public DeclarationNode? LastChild => Children.LastOrDefault();

    public DeclarationNode? FindFirstChild(Func<DeclarationNode, bool> predicate) => Children.FirstOrDefault(predicate);

    public DeclarationNode? FindLastChild(Func<DeclarationNode, bool> predicate) => Children.LastOrDefault(predicate);
}

public enum DeclarationScopeFeature
{
    None,
    Local,
    Global,
}

[Flags]
public enum DeclarationFeature
{
    None = 0,
    Deprecated = 0x01
}

public enum DeclarationVisibility
{
    Public,
    Protected,
    Private,
}

// TODO : refactor to use a record
public class LuaDeclaration(
    string name,
    int position,
    LuaElementPtr<LuaSyntaxElement> ptr,
    LuaType? declarationType,
    DeclarationScopeFeature scopeFeature = DeclarationScopeFeature.None,
    DeclarationFeature feature = DeclarationFeature.None,
    DeclarationVisibility visibility = DeclarationVisibility.Public
)
    : DeclarationNode(position)
{
    public string Name { get; } = name;

    public LuaElementPtr<LuaSyntaxElement> Ptr { get; } = ptr;

    public LuaType? DeclarationType = declarationType;

    public DeclarationScopeFeature ScopeFeature { get; internal init; } = scopeFeature;

    public DeclarationFeature Feature { get; internal set; } = feature;

    public bool IsDeprecated => Feature.HasFlag(DeclarationFeature.Deprecated);

    public DeclarationVisibility Visibility { get; internal set; } = visibility;

    public bool IsPublic => Visibility == DeclarationVisibility.Public;

    public bool IsProtected => Visibility == DeclarationVisibility.Protected;

    public bool IsPrivate => Visibility == DeclarationVisibility.Private;

    public virtual LuaDeclaration WithType(LuaType type) =>
        new(Name, Position, Ptr, type, ScopeFeature, Feature, Visibility);

    public virtual LuaDeclaration Instantiate(Dictionary<string, LuaType> genericMap)
    {
        if (DeclarationType is { } type)
        {
            return WithType(type.Instantiate(genericMap));
        }

        return this;
    }

    public override string ToString()
    {
        return $"{Name}";
    }
}

public class LocalDeclaration(
    string name,
    int position,
    LuaElementPtr<LuaLocalNameSyntax> localNamePtr,
    LuaType? declarationType
) : LuaDeclaration(name, position, localNamePtr.UpCast(), declarationType, DeclarationScopeFeature.Local)
{
    public LuaElementPtr<LuaLocalNameSyntax> LocalNamePtr => Ptr.Cast<LuaLocalNameSyntax>();

    public bool IsTypeDefine { get; internal set; }

    public override LocalDeclaration WithType(LuaType type) =>
        new(Name, Position, LocalNamePtr, type)
        {
            IsTypeDefine = IsTypeDefine
        };
}

public class GlobalDeclaration(
    string name,
    int position,
    LuaElementPtr<LuaNameExprSyntax> varNamePtr,
    LuaType? declarationType,
    DeclarationFeature feature = DeclarationFeature.None,
    DeclarationVisibility visibility = DeclarationVisibility.Public
) : LuaDeclaration(name, position, varNamePtr.UpCast(), declarationType, DeclarationScopeFeature.Global, feature,
    visibility)
{
    public LuaElementPtr<LuaNameExprSyntax> VarNamePtr => Ptr.Cast<LuaNameExprSyntax>();

    public bool IsTypeDefine { get; internal set; }

    public override GlobalDeclaration WithType(LuaType type) =>
        new(Name, Position, VarNamePtr, type, Feature, Visibility)
        {
            IsTypeDefine = IsTypeDefine,
        };
}

public class ParamDeclaration(
    string name,
    int position,
    LuaElementPtr<LuaSyntaxNode> paramPtr,
    LuaType? declarationType)
    : LuaDeclaration(name, position, paramPtr.UpCast(), declarationType, DeclarationScopeFeature.Local)
{
    public LuaElementPtr<LuaParamDefSyntax> ParamDefPtr => Ptr.Cast<LuaParamDefSyntax>();

    public LuaElementPtr<LuaDocTypedParamSyntax> TypedParamPtr => Ptr.Cast<LuaDocTypedParamSyntax>();

    public bool IsVararg => Name == "...";

    public override ParamDeclaration WithType(LuaType type) =>
        new(Name, Position, Ptr.Cast<LuaSyntaxNode>(), type);
}

public class MethodDeclaration(
    string name,
    int position,
    LuaElementPtr<LuaSyntaxNode> namePtr,
    LuaMethodType? method,
    LuaElementPtr<LuaFuncStatSyntax> funcStatPtr,
    DeclarationFeature feature = DeclarationFeature.None,
    DeclarationVisibility visibility = DeclarationVisibility.Public
) : LuaDeclaration(name, position, namePtr.UpCast(), method, feature: feature, visibility: visibility)
{
    public LuaElementPtr<LuaIndexExprSyntax> IndexExprPtr => Ptr.Cast<LuaIndexExprSyntax>();

    public LuaElementPtr<LuaFuncStatSyntax> FuncStatPtr => funcStatPtr;

    public override MethodDeclaration WithType(LuaType type) =>
        new(Name, Position, Ptr.Cast<LuaSyntaxNode>(), type as LuaMethodType, FuncStatPtr, Feature, Visibility);
}

public class NamedTypeDeclaration(
    string name,
    int position,
    LuaElementPtr<LuaDocTagNamedTypeSyntax> typeDefinePtr,
    LuaType type,
    NamedTypeKind kind)
    : LuaDeclaration(name, position, typeDefinePtr.UpCast(), type)
{
    public NamedTypeKind Kind { get; } = kind;

    public LuaElementPtr<LuaDocTagNamedTypeSyntax> TypeDefinePtr => Ptr.Cast<LuaDocTagNamedTypeSyntax>();

    public override NamedTypeDeclaration WithType(LuaType type) =>
        new(Name, Position, TypeDefinePtr, type, Kind);
}

public class DocFieldDeclaration(
    string name,
    int position,
    LuaElementPtr<LuaDocFieldSyntax> fieldDefPtr,
    LuaType? declarationType,
    DeclarationFeature feature = DeclarationFeature.None,
    DeclarationVisibility visibility = DeclarationVisibility.Public
) : LuaDeclaration(name, position, fieldDefPtr.UpCast(), declarationType, feature: feature, visibility: visibility)
{
    public LuaElementPtr<LuaDocFieldSyntax> FieldDefPtr => Ptr.Cast<LuaDocFieldSyntax>();

    public override DocFieldDeclaration WithType(LuaType type) =>
        new DocFieldDeclaration(Name, Position, FieldDefPtr, type, Feature, Visibility);
}

public class TableFieldDeclaration(
    string name,
    int position,
    LuaElementPtr<LuaTableFieldSyntax> tableFieldPtr,
    LuaType? declarationType) : LuaDeclaration(name, position, tableFieldPtr.UpCast(), declarationType)
{
    public LuaElementPtr<LuaTableFieldSyntax> TableFieldPtr => Ptr.Cast<LuaTableFieldSyntax>();

    public override TableFieldDeclaration WithType(LuaType type) =>
        new(Name, Position, TableFieldPtr, type);
}

public class EnumFieldDeclaration(
    string name,
    int position,
    LuaElementPtr<LuaDocTagEnumFieldSyntax> enumFieldDefPtr,
    LuaType? declarationType) : LuaDeclaration(name, position, enumFieldDefPtr.UpCast(), declarationType)
{
    public LuaElementPtr<LuaDocTagEnumFieldSyntax> EnumFieldDefPtr => Ptr.Cast<LuaDocTagEnumFieldSyntax>();

    public override EnumFieldDeclaration WithType(LuaType type) =>
        new(Name, Position, EnumFieldDefPtr, type);
}

public class GenericParamDeclaration(
    string name,
    int position,
    LuaElementPtr<LuaDocGenericParamSyntax> genericParamDefPtr,
    LuaType? baseType,
    bool variadic = false) : LuaDeclaration(name, position, genericParamDefPtr.UpCast(), baseType)
{
    public LuaElementPtr<LuaDocGenericParamSyntax> GenericParameterDefPtr => Ptr.Cast<LuaDocGenericParamSyntax>();

    public bool Variadic { get; set; } = variadic;

    public override GenericParamDeclaration WithType(LuaType type) =>
        new(Name, Position, GenericParameterDefPtr, type, Variadic);
}

public class IndexDeclaration(
    string name,
    int position,
    LuaElementPtr<LuaIndexExprSyntax> indexExprPtr,
    LuaElementPtr<LuaExprSyntax> valueExprPtr,
    LuaType? declarationType,
    DeclarationFeature feature = DeclarationFeature.None,
    DeclarationVisibility visibility = DeclarationVisibility.Public)
    : LuaDeclaration(name, position, indexExprPtr.UpCast(), declarationType, feature: feature, visibility: visibility)
{
    public LuaElementPtr<LuaIndexExprSyntax> IndexExprPtr => Ptr.Cast<LuaIndexExprSyntax>();

    public LuaElementPtr<LuaExprSyntax> ValueExprPtr => valueExprPtr;

    public override IndexDeclaration WithType(LuaType type) =>
        new(Name, Position, IndexExprPtr, ValueExprPtr, type, Feature, Visibility);
}

public class TypeIndexDeclaration(
    LuaType keyType,
    LuaType valueType,
    LuaElementPtr<LuaDocFieldSyntax> fieldDefPtr)
    : LuaDeclaration(string.Empty, 0, fieldDefPtr.UpCast(), valueType)
{
    public LuaElementPtr<LuaDocFieldSyntax> FieldDefPtr => Ptr.Cast<LuaDocFieldSyntax>();

    public LuaType KeyType => keyType;

    public LuaType ValueType => DeclarationType!;

    public override TypeIndexDeclaration WithType(LuaType type) => new(KeyType, ValueType, FieldDefPtr);

    public override LuaDeclaration Instantiate(Dictionary<string, LuaType> genericMap) =>
        new TypeIndexDeclaration(KeyType.Instantiate(genericMap), ValueType.Instantiate(genericMap), FieldDefPtr);
}

public class TypeOpDeclaration(LuaType? luaType, LuaElementPtr<LuaDocTagOperatorSyntax> opFieldPtr)
    : LuaDeclaration(string.Empty, 0, opFieldPtr.UpCast(), luaType)
{
    public LuaElementPtr<LuaDocTagOperatorSyntax> OpFieldPtr => Ptr.Cast<LuaDocTagOperatorSyntax>();

    public override TypeOpDeclaration WithType(LuaType type) => new(type, OpFieldPtr);
}

public class TupleMemberDeclaration(int i, LuaType? luaType, LuaElementPtr<LuaDocTypeSyntax> typePtr)
    : LuaDeclaration($"[{i}]", 0, typePtr.UpCast(), luaType)
{
    public int Index { get; } = i;

    public LuaElementPtr<LuaDocTypeSyntax> TypePtr => Ptr.Cast<LuaDocTypeSyntax>();

    public override TupleMemberDeclaration WithType(LuaType type) => new(Index, type, TypePtr);
}

public class VirtualDeclaration(
    string name,
    LuaType? declarationType
) : LuaDeclaration(name, 0, LuaElementPtr<LuaSyntaxElement>.Empty, declarationType)
{
    public override VirtualDeclaration WithType(LuaType type) =>
        new(Name, type);
}
