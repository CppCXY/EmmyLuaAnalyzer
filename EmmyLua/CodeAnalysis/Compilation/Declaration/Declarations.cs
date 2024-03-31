using EmmyLua.CodeAnalysis.Compilation.Type;
using EmmyLua.CodeAnalysis.Syntax.Node;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace EmmyLua.CodeAnalysis.Compilation.Declaration;

public class DeclarationNode(int position)
{
    public DeclarationNode? Prev { get; set; }

    public DeclarationNode? Next { get; set; }

    public DeclarationNodeContainer? Parent { get; set; } = null;

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

public enum DeclarationFeature
{
    None,
    Local,
    Global,
}

public class LuaDeclaration(
    string name,
    LuaSyntaxNodePtr<LuaSyntaxNode> ptr,
    LuaType? declarationType,
    DeclarationFeature feature = DeclarationFeature.None
)
    : DeclarationNode(ptr.Range.StartOffset)
{
    public string Name { get; } = name;

    public LuaSyntaxNodePtr<LuaSyntaxNode> Ptr { get; } = ptr;

    public LuaType? DeclarationType = declarationType;

    public DeclarationFeature Feature { get; internal init; } = feature;

    public virtual LuaDeclaration WithType(LuaType type) =>
        new(Name, Ptr, type, Feature);

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

public class LocalLuaDeclaration(
    string name,
    LuaSyntaxNodePtr<LuaLocalNameSyntax> localNamePtr,
    LuaType? declarationType
) : LuaDeclaration(name, localNamePtr.UpCast(), declarationType, DeclarationFeature.Local)
{
    public LuaSyntaxNodePtr<LuaLocalNameSyntax> LocalNamePtr => Ptr.Cast<LuaLocalNameSyntax>();

    public bool IsTypeDefine { get; internal set; } = false;

    public override LocalLuaDeclaration WithType(LuaType type) =>
        new(Name, LocalNamePtr, type)
        {
            IsTypeDefine = IsTypeDefine
        };
}

public class GlobalLuaDeclaration(
    string name,
    LuaSyntaxNodePtr<LuaNameExprSyntax> varNamePtr,
    LuaType? declarationType
) : LuaDeclaration(name, varNamePtr.UpCast(), declarationType, DeclarationFeature.Global)
{
    public LuaSyntaxNodePtr<LuaNameExprSyntax> VarNamePtr => Ptr.Cast<LuaNameExprSyntax>();

    public bool IsTypeDefine { get; internal set; } = false;

    public override GlobalLuaDeclaration WithType(LuaType type) =>
        new(Name, VarNamePtr, type)
        {
            IsTypeDefine = IsTypeDefine
        };
}

public class ParameterLuaDeclaration(
    string name,
    LuaSyntaxNodePtr<LuaSyntaxNode> paramPtr,
    LuaType? declarationType) : LuaDeclaration(name, paramPtr.UpCast(), declarationType, DeclarationFeature.Local)
{
    public LuaSyntaxNodePtr<LuaParamDefSyntax> ParamDefPtr => Ptr.Cast<LuaParamDefSyntax>();

    public LuaSyntaxNodePtr<LuaDocTypedParamSyntax> TypedParamPtr => Ptr.Cast<LuaDocTypedParamSyntax>();

    public override ParameterLuaDeclaration WithType(LuaType type) =>
        new(Name, Ptr, type);
}

public class MethodLuaDeclaration(
    string name,
    LuaSyntaxNodePtr<LuaSyntaxNode> namePtr,
    LuaMethodType? method,
    LuaSyntaxNodePtr<LuaFuncStatSyntax> funcStatPtr
) : LuaDeclaration(name, namePtr, method)
{
    public LuaSyntaxNodePtr<LuaIndexExprSyntax> IndexExprPtr => Ptr.Cast<LuaIndexExprSyntax>();

    public LuaSyntaxNodePtr<LuaFuncStatSyntax> FuncStatPtr => funcStatPtr;

    public override MethodLuaDeclaration WithType(LuaType type) =>
        new(Name, Ptr, type as LuaMethodType, FuncStatPtr);
}

public class NamedTypeLuaDeclaration(
    string name,
    LuaSyntaxNodePtr<LuaDocTagNamedTypeSyntax> typeDefinePtr,
    LuaType type,
    NamedTypeKind kind)
    : LuaDeclaration(name, typeDefinePtr.UpCast(), type)
{
    public NamedTypeKind Kind { get; } = kind;

    public LuaSyntaxNodePtr<LuaDocTagNamedTypeSyntax> TypeDefinePtr => Ptr.Cast<LuaDocTagNamedTypeSyntax>();

    public override NamedTypeLuaDeclaration WithType(LuaType type) =>
        new (Name, TypeDefinePtr, type, Kind);
}

public class DocFieldLuaDeclaration(
    string name,
    LuaSyntaxNodePtr<LuaDocFieldSyntax> fieldDefPtr,
    LuaType? declarationType) : LuaDeclaration(name, fieldDefPtr.UpCast(), declarationType)
{
    public LuaSyntaxNodePtr<LuaDocFieldSyntax> FieldDefPtr => Ptr.Cast<LuaDocFieldSyntax>();

    public override DocFieldLuaDeclaration WithType(LuaType type) =>
        new DocFieldLuaDeclaration(Name, FieldDefPtr, type);
}

public class TableFieldLuaDeclaration(
    string name,
    LuaSyntaxNodePtr<LuaTableFieldSyntax> tableFieldPtr,
    LuaType? declarationType) : LuaDeclaration(name, tableFieldPtr.UpCast(), declarationType)
{
    public LuaSyntaxNodePtr<LuaTableFieldSyntax> TableFieldPtr => Ptr.Cast<LuaTableFieldSyntax>();

    public override TableFieldLuaDeclaration WithType(LuaType type) =>
        new(Name, TableFieldPtr, type);
}

public class EnumFieldLuaDeclaration(
    string name,
    LuaSyntaxNodePtr<LuaDocTagEnumFieldSyntax> enumFieldDefPtr,
    LuaType? declarationType) : LuaDeclaration(name, enumFieldDefPtr.UpCast(), declarationType)
{
    public LuaSyntaxNodePtr<LuaDocTagEnumFieldSyntax> EnumFieldDefPtr => Ptr.Cast<LuaDocTagEnumFieldSyntax>();

    public override EnumFieldLuaDeclaration WithType(LuaType type) =>
        new(Name, EnumFieldDefPtr, type);
}

public class GenericParameterLuaDeclaration(
    string name,
    LuaSyntaxNodePtr<LuaDocGenericParamSyntax> genericParamDefPtr,
    LuaType? baseType) : LuaDeclaration(name, genericParamDefPtr.UpCast(), baseType)
{
    public LuaSyntaxNodePtr<LuaDocGenericParamSyntax> GenericParameterDefPtr => Ptr.Cast<LuaDocGenericParamSyntax>();

    public override GenericParameterLuaDeclaration WithType(LuaType type) =>
        new(Name, GenericParameterDefPtr, type);
}

public class IndexLuaDeclaration(
    string name,
    LuaSyntaxNodePtr<LuaIndexExprSyntax> indexExprPtr,
    LuaType? declarationType) : LuaDeclaration(name, indexExprPtr.UpCast(), declarationType)
{
    public LuaSyntaxNodePtr<LuaIndexExprSyntax> IndexExprPtr => Ptr.Cast<LuaIndexExprSyntax>();

    public override IndexLuaDeclaration WithType(LuaType type) =>
        new(Name, IndexExprPtr, type);
}

public class LabelLuaDeclaration(
    string name,
    LuaSyntaxNodePtr<LuaLabelStatSyntax> labelStatPtr) : LuaDeclaration(name, labelStatPtr.UpCast(), null)
{
    public LuaSyntaxNodePtr<LuaLabelStatSyntax> LabelStatPtr => Ptr.Cast<LuaLabelStatSyntax>();

    public override LabelLuaDeclaration WithType(LuaType type) =>
        new LabelLuaDeclaration(Name, LabelStatPtr);
}

public class TypeIndexDeclaration(
    LuaType keyType,
    LuaType valueType,
    LuaSyntaxNodePtr<LuaDocFieldSyntax> fieldDefPtr)
    : LuaDeclaration(string.Empty, fieldDefPtr.UpCast(), valueType)
{
    public LuaSyntaxNodePtr<LuaDocFieldSyntax> FieldDefPtr => Ptr.Cast<LuaDocFieldSyntax>();

    public LuaType KeyType => keyType;

    public LuaType ValueType => DeclarationType!;

    public override TypeIndexDeclaration WithType(LuaType type) => new(KeyType, ValueType, FieldDefPtr);

    public override LuaDeclaration Instantiate(Dictionary<string, LuaType> genericMap) =>
        new TypeIndexDeclaration(KeyType.Instantiate(genericMap), ValueType.Instantiate(genericMap), FieldDefPtr);
}

public class TypeOpDeclaration(LuaType? luaType, LuaSyntaxNodePtr<LuaDocTagOperatorSyntax> opFieldPtr)
    : LuaDeclaration(string.Empty, opFieldPtr.UpCast(), luaType)
{
    public LuaSyntaxNodePtr<LuaDocTagOperatorSyntax> OpFieldPtr => Ptr.Cast<LuaDocTagOperatorSyntax>();

    public override TypeOpDeclaration WithType(LuaType type) => new (type, OpFieldPtr);
}

public class TupleMemberDeclaration(int i, LuaType? luaType, LuaSyntaxNodePtr<LuaDocTypeSyntax> typePtr)
    : LuaDeclaration($"[{i}]", typePtr.UpCast(), luaType)
{
    public int Index { get; } = i;

    public LuaSyntaxNodePtr<LuaDocTypeSyntax> TypePtr => Ptr.Cast<LuaDocTypeSyntax>();

    public override TupleMemberDeclaration WithType(LuaType type) => new(Index, type, TypePtr);
}

public class VirtualDeclaration(
    string name,
    LuaType? declarationType
) : LuaDeclaration(name, LuaSyntaxNodePtr<LuaSyntaxNode>.Empty, declarationType)
{
    public override VirtualDeclaration WithType(LuaType type) =>
        new(Name, type);
}
