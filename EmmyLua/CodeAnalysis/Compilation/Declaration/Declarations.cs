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
    int position,
    LuaSyntaxElement? syntaxElement,
    LuaType? declarationType,
    DeclarationFeature feature = DeclarationFeature.None
)
    : DeclarationNode(position)
{
    public string Name { get; } = name;

    public LuaSyntaxElement? SyntaxElement { get; } = syntaxElement;

    public LuaType? DeclarationType = declarationType;

    public DeclarationFeature Feature { get; internal init; } = feature;

    public virtual LuaDeclaration WithType(LuaType type) =>
        new LuaDeclaration(Name, Position, SyntaxElement, type, Feature);

    public override string ToString()
    {
        return $"{Name}";
    }
}

public class LocalLuaDeclaration(
    string name,
    int position,
    LuaLocalNameSyntax localName,
    LuaType? declarationType
) : LuaDeclaration(name, position, localName, declarationType, DeclarationFeature.Local)
{
    public LuaLocalNameSyntax LocalName => localName;

    public bool IsConst => localName.Attribute?.IsConst == true;

    public bool IsClose => localName.Attribute?.IsClose == true;

    public bool IsTypeDefine { get; internal set; } = false;

    public override LocalLuaDeclaration WithType(LuaType type) =>
        new LocalLuaDeclaration(Name, Position, LocalName, type);
}

public class GlobalLuaDeclaration(
    string name,
    int position,
    LuaNameExprSyntax varName,
    LuaType? declarationType
) : LuaDeclaration(name, position, varName, declarationType, DeclarationFeature.Global)
{
    public LuaNameExprSyntax VarName => varName;

    public bool IsTypeDefine { get; internal set; } = false;

    public override GlobalLuaDeclaration WithType(LuaType type)
    {
        var global = new GlobalLuaDeclaration(Name, Position, VarName, type);
        global.IsTypeDefine = IsTypeDefine;
        return global;
    }
}

public class DocParameterLuaDeclaration(
    string name,
    int position,
    LuaSyntaxToken nameOrVararg,
    LuaType? declarationType) : LuaDeclaration(name, position, nameOrVararg, declarationType)
{
    public LuaNameToken? ParamName => SyntaxElement as LuaNameToken;

    public LuaDotsToken? Vararg => SyntaxElement as LuaDotsToken;
}

public class ParameterLuaDeclaration(
    string name,
    int position,
    LuaSyntaxElement? element,
    LuaType? declarationType) : LuaDeclaration(name, position, element, declarationType, DeclarationFeature.Local)
{
    public LuaParamDefSyntax? ParamDef => SyntaxElement as LuaParamDefSyntax;

    public LuaDocTagTypedParamSyntax? TypedParamDef => SyntaxElement as LuaDocTagTypedParamSyntax;

    public override ParameterLuaDeclaration WithType(LuaType type) =>
        new ParameterLuaDeclaration(Name, Position, SyntaxElement, type);
}

public class MethodLuaDeclaration(
    string name,
    int position,
    LuaSyntaxElement element,
    LuaMethodType? method,
    LuaClosureExprSyntax closureExpr
) : LuaDeclaration(name, position, element, method)
{
    public LuaFuncStatSyntax? MethodDef => SyntaxElement?.Parent as LuaFuncStatSyntax;

    public LuaIndexExprSyntax? IndexExpr => SyntaxElement as LuaIndexExprSyntax;

    public LuaClosureExprSyntax ClosureExpr => closureExpr;

    public bool IsLocal => MethodDef?.IsLocal ?? false;

    public override MethodLuaDeclaration WithType(LuaType type) =>
        new MethodLuaDeclaration(Name, Position, SyntaxElement!, type as LuaMethodType, ClosureExpr);
}

public class NamedTypeLuaDeclaration(
    string name,
    int position,
    LuaNameToken nameToken,
    LuaType type)
    : LuaDeclaration(name, position, nameToken, type)
{
    public LuaNameToken NameToken => nameToken;

    public override NamedTypeLuaDeclaration WithType(LuaType type) =>
        new NamedTypeLuaDeclaration(Name, Position, NameToken, type);
}

public class DocFieldLuaDeclaration(
    string name,
    int position,
    LuaSyntaxElement fieldDef,
    LuaType? declarationType) : LuaDeclaration(name, position, fieldDef, declarationType)
{
    public LuaDocTagFieldSyntax? FieldDef => SyntaxElement as LuaDocTagFieldSyntax;

    public LuaDocTagTypedFieldSyntax? TypedFieldDef => SyntaxElement as LuaDocTagTypedFieldSyntax;

    public override DocFieldLuaDeclaration WithType(LuaType type) =>
        new DocFieldLuaDeclaration(Name, Position, SyntaxElement!, type);
}

public class TableFieldLuaDeclaration(
    string name,
    int position,
    LuaTableFieldSyntax tableField,
    LuaType? declarationType) : LuaDeclaration(name, position, tableField, declarationType)
{
    public LuaTableFieldSyntax TableField => tableField;

    public override TableFieldLuaDeclaration WithType(LuaType type) =>
        new TableFieldLuaDeclaration(Name, Position, TableField, type);
}

public class EnumFieldLuaDeclaration(
    string name,
    int position,
    LuaDocTagEnumFieldSyntax enumFieldDef,
    LuaType? declarationType) : LuaDeclaration(name, position, enumFieldDef, declarationType)
{
    public LuaDocTagEnumFieldSyntax EnumFieldDef => enumFieldDef;

    public override EnumFieldLuaDeclaration WithType(LuaType type) =>
        new EnumFieldLuaDeclaration(Name, Position, EnumFieldDef, type);
}

public class GenericParameterLuaDeclaration(
    string name,
    int position,
    LuaDocGenericParamSyntax genericParameterDef,
    LuaType? baseType) : LuaDeclaration(name, position, genericParameterDef, baseType)
{
    public LuaDocGenericParamSyntax GenericParameterDef => genericParameterDef;

    public override GenericParameterLuaDeclaration WithType(LuaType type) =>
        new GenericParameterLuaDeclaration(Name, Position, GenericParameterDef, type);
}

public class IndexLuaDeclaration(
    string name,
    int position,
    LuaIndexExprSyntax indexExpr,
    LuaType? declarationType) : LuaDeclaration(name, position, indexExpr, declarationType)
{
    public LuaIndexExprSyntax IndexExpr => indexExpr;

    public override IndexLuaDeclaration WithType(LuaType type) =>
        new IndexLuaDeclaration(Name, Position, IndexExpr, type);
}

public class LabelLuaDeclaration(
    string name,
    int position,
    LuaLabelStatSyntax labelStat) : LuaDeclaration(name, position, labelStat, null)
{
    public LuaLabelStatSyntax LabelStat => labelStat;

    public override LabelLuaDeclaration WithType(LuaType type) =>
        new LabelLuaDeclaration(Name, Position, LabelStat);
}

public class VirtualDeclaration(LuaType? luaType, LuaSyntaxElement? element = null)
    : LuaDeclaration(string.Empty, 0, element, luaType)
{
    public override VirtualDeclaration WithType(LuaType type) =>
        new VirtualDeclaration(type, SyntaxElement);
}

public class TypeIndexDeclaration(LuaType? luaType, LuaDocTagFieldSyntax? field)
    : VirtualDeclaration(luaType, field)
{
    public LuaDocTagFieldSyntax? Field => SyntaxElement as LuaDocTagFieldSyntax;

    public override VirtualDeclaration WithType(LuaType type) => new TypeIndexDeclaration(type, Field);
}

public class TypeOpDeclaration(LuaType? luaType, LuaDocTagOperatorSyntax? opField)
    : VirtualDeclaration(luaType, opField)
{
    public LuaDocTagOperatorSyntax? OpField => SyntaxElement as LuaDocTagOperatorSyntax;

    public override VirtualDeclaration WithType(LuaType type) => new TypeOpDeclaration(type, OpField);
}
