using EmmyLua.CodeAnalysis.Compilation.Type;
using EmmyLua.CodeAnalysis.Syntax.Node;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace EmmyLua.CodeAnalysis.Compilation.Symbol;

public class Declaration(
    string name,
    int position,
    LuaSyntaxElement? syntaxElement,
    LuaType? declarationType,
    SymbolFeature feature = SymbolFeature.None
)
    : Symbol(name, position, syntaxElement, SymbolKind.Declaration, declarationType, feature);

public class LocalDeclaration(
    string name,
    int position,
    LuaLocalNameSyntax localName,
    LuaType? declarationType
) : Declaration(name, position, localName, declarationType, SymbolFeature.Local)
{
    public LuaLocalNameSyntax LocalName => localName;

    public bool IsConst => localName.Attribute?.IsConst == true;

    public bool IsClose => localName.Attribute?.IsClose == true;
}

public class GlobalDeclaration(
    string name,
    int position,
    LuaNameExprSyntax varName,
    LuaType? declarationType) : Declaration(name, position, varName, declarationType, SymbolFeature.Global)
{
    public LuaNameExprSyntax VarName => varName;
}

public class DocParameterDeclaration(
    string name,
    int position,
    LuaSyntaxToken nameOrVararg,
    LuaType? declarationType) : Declaration(name, position, nameOrVararg, declarationType)
{
    public LuaNameToken? ParamName => SyntaxElement as LuaNameToken;

    public LuaDotsToken? Vararg => SyntaxElement as LuaDotsToken;
}

public class ParameterDeclaration(
    string name,
    int position,
    LuaSyntaxElement? element,
    LuaType? declarationType) : Declaration(name, position, element, declarationType, SymbolFeature.Local)
{
    public LuaParamDefSyntax? ParamDef => SyntaxElement as LuaParamDefSyntax;

    public LuaDocTagTypedParamSyntax? TypedParamDef => SyntaxElement as LuaDocTagTypedParamSyntax;

    public ParameterDeclaration WithType(LuaType type) => new ParameterDeclaration(Name, Position, SyntaxElement, type);
}

public class MethodDeclaration(
    string name,
    int position,
    LuaSyntaxElement element,
    LuaMethodType? method,
    LuaClosureExprSyntax closureExpr
) : Declaration(name, position, element, method)
{
    public LuaFuncStatSyntax? MethodDef => SyntaxElement?.Parent as LuaFuncStatSyntax;

    public LuaIndexExprSyntax? IndexExpr => SyntaxElement as LuaIndexExprSyntax;

    public LuaClosureExprSyntax ClosureExpr => closureExpr;
}

public class NamedTypeDeclaration(
    string name,
    int position,
    LuaNameToken nameToken,
    LuaType type)
    : Declaration(name, position, nameToken, type)
{
    public LuaNameToken NameToken => nameToken;
}

public class DocFieldDeclaration(
    string name,
    int position,
    LuaSyntaxElement fieldDef,
    LuaType? declarationType) : Declaration(name, position, fieldDef, declarationType)
{
    public LuaDocTagFieldSyntax? FieldDef => SyntaxElement as LuaDocTagFieldSyntax;

    public LuaDocTagTypedFieldSyntax? TypedFieldDef => SyntaxElement as LuaDocTagTypedFieldSyntax;
}

public class TableFieldDeclaration(
    string name,
    int position,
    LuaTableFieldSyntax tableField,
    LuaType? declarationType) : Declaration(name, position, tableField, declarationType)
{
    public LuaTableFieldSyntax TableField => tableField;
}

public class EnumFieldDeclaration(
    string name,
    int position,
    LuaDocTagEnumFieldSyntax enumFieldDef,
    LuaType? declarationType) : Declaration(name, position, enumFieldDef, declarationType)
{
    public LuaDocTagEnumFieldSyntax EnumFieldDef => enumFieldDef;
}

public class GenericParameterDeclaration(
    string name,
    int position,
    LuaDocGenericParamSyntax genericParameterDef,
    LuaType? baseType) : Declaration(name, position, genericParameterDef, baseType)
{
    public LuaDocGenericParamSyntax GenericParameterDef => genericParameterDef;
}

public class IndexDeclaration(
    string name,
    int position,
    LuaIndexExprSyntax indexExpr,
    LuaType? declarationType) : Declaration(name, position, indexExpr, declarationType)
{
    public LuaIndexExprSyntax IndexExpr => indexExpr;
}

public class LabelDeclaration(
    string name,
    int position,
    LuaLabelStatSyntax labelStat) : Declaration(name, position, labelStat, null)
{
    public LuaLabelStatSyntax LabelStat => labelStat;
}

