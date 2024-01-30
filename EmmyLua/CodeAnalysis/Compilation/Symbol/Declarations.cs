using EmmyLua.CodeAnalysis.Compilation.Type;
using EmmyLua.CodeAnalysis.Syntax.Node;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace EmmyLua.CodeAnalysis.Compilation.Symbol;

public class Declaration(
    string name,
    int position,
    LuaSyntaxElement? syntaxElement,
    ILuaType? declarationType,
    SymbolFeature feature = SymbolFeature.None
    )
    : Symbol(name, position, syntaxElement, SymbolKind.Declaration, null, null, declarationType, feature);

public class LocalDeclaration(
    string name,
    int position,
    LuaLocalNameSyntax localName,
    ILuaType? declarationType) : Declaration(name, position, localName, declarationType, SymbolFeature.Local)
{
    public LuaLocalNameSyntax LocalName => localName;

    public bool IsConst => localName.Attribute?.IsConst == true;

    public bool IsClose => localName.Attribute?.IsClose == true;
}

public class GlobalDeclaration(
    string name,
    int position,
    LuaNameExprSyntax nameExpr,
    ILuaType? declarationType) : Declaration(name, position, nameExpr, declarationType, SymbolFeature.Global)
{
    public LuaNameExprSyntax NameSyntax => nameExpr;
}

public class DocParameterDeclaration(
    string name,
    int position,
    LuaSyntaxToken nameOrVararg,
    ILuaType? declarationType) : Declaration(name, position, nameOrVararg, declarationType)
{
    public LuaNameToken? ParamName => SyntaxElement as LuaNameToken;

    public LuaDotsToken? Vararg => SyntaxElement as LuaDotsToken;
}

public class ParameterDeclaration(
    string name,
    int position,
    LuaParamDefSyntax paramDef,
    ILuaType? declarationType) : Declaration(name, position, paramDef, declarationType, SymbolFeature.Local)
{
    public LuaParamDefSyntax ParamDef => paramDef;
}

public class MethodDeclaration(
    string name,
    int position,
    LuaSyntaxElement element,
    LuaMethod declarationType,
    LuaFuncBodySyntax funcBodySyntax
    ) : Declaration(name, position, element, declarationType)
{
    public LuaFuncStatSyntax? MethodDef => SyntaxElement?.Parent as LuaFuncStatSyntax;

    public LuaIndexExprSyntax? IndexExprSyntax => SyntaxElement as LuaIndexExprSyntax;

    public LuaMethod MethodType => declarationType;

    public LuaFuncBodySyntax FuncBodySyntax => funcBodySyntax;
}

public class ClassDeclaration(
    string name,
    int position,
    LuaDocTagClassSyntax classDef,
    LuaClass declarationType) : Declaration(name, position, classDef, declarationType)
{
    public LuaDocTagClassSyntax ClassDef => classDef;

    public LuaClass ClassType => declarationType;
}

public class EnumDeclaration(
    string name,
    int position,
    LuaDocTagEnumSyntax enumDef,
    LuaEnum declarationType) : Declaration(name, position, enumDef, declarationType)
{
    public LuaDocTagEnumSyntax EnumDef => enumDef;

    public LuaEnum EnumType => declarationType;
}

public class InterfaceDeclaration(
    string name,
    int position,
    LuaDocTagInterfaceSyntax interfaceDef,
    LuaInterface declarationType) : Declaration(name, position, interfaceDef, declarationType)
{
    public LuaDocTagInterfaceSyntax InterfaceDef => interfaceDef;

    public LuaInterface InterfaceType => declarationType;
}

public class AliasDeclaration(
    string name,
    int position,
    LuaDocTagAliasSyntax aliasDef,
    ILuaType? declarationType) : Declaration(name, position, aliasDef, declarationType)
{
    public LuaDocTagAliasSyntax AliasDef => aliasDef;
}

public class DocFieldDeclaration(
    string name,
    int position,
    LuaSyntaxElement fieldDef,
    ILuaType? declarationType) : Declaration(name, position, fieldDef, declarationType)
{
    public LuaDocTagFieldSyntax? FieldDef => SyntaxElement as LuaDocTagFieldSyntax;

    public LuaDocTagTypedFieldSyntax? TypedFieldDef => SyntaxElement as LuaDocTagTypedFieldSyntax;
}

public class TableFieldDeclaration(
    string name,
    int position,
    LuaTableFieldSyntax tableField,
    ILuaType? declarationType) : Declaration(name, position, tableField, declarationType)
{
    public LuaTableFieldSyntax TableField => tableField;
}

public class EnumFieldDeclaration(
    string name,
    int position,
    LuaDocTagEnumFieldSyntax enumFieldDef,
    ILuaType? declarationType) : Declaration(name, position, enumFieldDef, declarationType)
{
    public LuaDocTagEnumFieldSyntax EnumFieldDef => enumFieldDef;
}

public class GenericParameterDeclaration(
    string name,
    int position,
    LuaDocGenericParamSyntax genericParameterDef,
    ILuaType? baseType) : Declaration(name, position, genericParameterDef, baseType)
{
    public LuaDocGenericParamSyntax GenericParameterDef => genericParameterDef;
}

public class IndexDeclaration(
    string name,
    int position,
    LuaIndexExprSyntax indexExpr,
    ILuaType? declarationType) : Declaration(name, position, indexExpr, declarationType)
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
