using EmmyLua.CodeAnalysis.Compilation.Type;
using EmmyLua.CodeAnalysis.Syntax.Node;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace EmmyLua.CodeAnalysis.Compilation.Symbol;

public class LuaDeclaration(
    string name,
    int position,
    LuaSyntaxElement? syntaxElement,
    LuaType? declarationType,
    SymbolFeature feature = SymbolFeature.None
)
    : LuaSymbol(name, position, syntaxElement, SymbolKind.Declaration, declarationType, feature);

public class LocalLuaDeclaration(
    string name,
    int position,
    LuaLocalNameSyntax localName,
    LuaType? declarationType
) : LuaDeclaration(name, position, localName, declarationType, SymbolFeature.Local)
{
    public LuaLocalNameSyntax LocalName => localName;

    public bool IsConst => localName.Attribute?.IsConst == true;

    public bool IsClose => localName.Attribute?.IsClose == true;
}

public class GlobalLuaDeclaration(
    string name,
    int position,
    LuaNameExprSyntax varName,
    LuaType? declarationType) : LuaDeclaration(name, position, varName, declarationType, SymbolFeature.Global)
{
    public LuaNameExprSyntax VarName => varName;
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
    LuaType? declarationType) : LuaDeclaration(name, position, element, declarationType, SymbolFeature.Local)
{
    public LuaParamDefSyntax? ParamDef => SyntaxElement as LuaParamDefSyntax;

    public LuaDocTagTypedParamSyntax? TypedParamDef => SyntaxElement as LuaDocTagTypedParamSyntax;

    public ParameterLuaDeclaration WithType(LuaType type) => new ParameterLuaDeclaration(Name, Position, SyntaxElement, type);
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
}

public class NamedTypeLuaDeclaration(
    string name,
    int position,
    LuaNameToken nameToken,
    LuaType type)
    : LuaDeclaration(name, position, nameToken, type)
{
    public LuaNameToken NameToken => nameToken;
}

public class DocFieldLuaDeclaration(
    string name,
    int position,
    LuaSyntaxElement fieldDef,
    LuaType? declarationType) : LuaDeclaration(name, position, fieldDef, declarationType)
{
    public LuaDocTagFieldSyntax? FieldDef => SyntaxElement as LuaDocTagFieldSyntax;

    public LuaDocTagTypedFieldSyntax? TypedFieldDef => SyntaxElement as LuaDocTagTypedFieldSyntax;
}

public class TableFieldLuaDeclaration(
    string name,
    int position,
    LuaTableFieldSyntax tableField,
    LuaType? declarationType) : LuaDeclaration(name, position, tableField, declarationType)
{
    public LuaTableFieldSyntax TableField => tableField;
}

public class EnumFieldLuaDeclaration(
    string name,
    int position,
    LuaDocTagEnumFieldSyntax enumFieldDef,
    LuaType? declarationType) : LuaDeclaration(name, position, enumFieldDef, declarationType)
{
    public LuaDocTagEnumFieldSyntax EnumFieldDef => enumFieldDef;
}

public class GenericParameterLuaDeclaration(
    string name,
    int position,
    LuaDocGenericParamSyntax genericParameterDef,
    LuaType? baseType) : LuaDeclaration(name, position, genericParameterDef, baseType)
{
    public LuaDocGenericParamSyntax GenericParameterDef => genericParameterDef;
}

public class IndexLuaDeclaration(
    string name,
    int position,
    LuaIndexExprSyntax indexExpr,
    LuaType? declarationType) : LuaDeclaration(name, position, indexExpr, declarationType)
{
    public LuaIndexExprSyntax IndexExpr => indexExpr;
}

public class LabelLuaDeclaration(
    string name,
    int position,
    LuaLabelStatSyntax labelStat) : LuaDeclaration(name, position, labelStat, null)
{
    public LuaLabelStatSyntax LabelStat => labelStat;
}

