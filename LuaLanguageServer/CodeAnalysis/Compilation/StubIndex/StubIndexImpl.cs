using LuaLanguageServer.CodeAnalysis.Syntax.Node;
using LuaLanguageServer.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace LuaLanguageServer.CodeAnalysis.Compilation.StubIndex;

public class StubIndexImpl
{
    public StubIndex<string, LuaShortName> ShortNameIndex { get;} = new();

    public StubIndex<LuaSyntaxElement, LuaMember> Members { get; } = new();

    public StubIndex<LuaSyntaxElement, LuaDocAttached> Attached { get; } = new();
}

public abstract record LuaShortName
{
    public record Class(LuaDocClassSyntax ClassSyntax) : LuaShortName;

    public record Enum(LuaDocEnumSyntax EnumSyntax) : LuaShortName;

    public record Alias(LuaDocAliasSyntax AliasSyntax) : LuaShortName;

    public record Interface(LuaDocInterfaceSyntax InterfaceSyntax) : LuaShortName;

    public record Field(LuaDocFieldSyntax FieldSyntax) : LuaShortName;

    public record ClassField(LuaDocTypedFieldSyntax FieldSyntax) : LuaShortName;

    public record EnumField(LuaDocEnumFieldSyntax EnumFieldSyntax) : LuaShortName;

    public record InterfaceField(LuaDocTypedFieldSyntax FieldSyntax) : LuaShortName;

    public record Local(LuaLocalNameSyntax LocalSName, LuaExprSyntax? Expr, int ReturnId) : LuaShortName;

    public record VarDef(LuaExprSyntax Var, LuaExprSyntax? Expr, int ReturnId) : LuaShortName;

    public record Param(LuaParamDefSyntax ParamDef) : LuaShortName;

    public record TableField(LuaTableFieldSyntax TableFieldSyntax) : LuaShortName;

    public record Label(LuaLabelStatSyntax LabelStatSyntax) : LuaShortName;

    public record Goto(LuaGotoStatSyntax GotoStatSyntax) : LuaShortName;

    public record Function(LuaFuncStatSyntax FuncStatSyntax) : LuaShortName;

    public record Generic(LuaDocTypedParamSyntax DocGenericParamSyntax) : LuaShortName;
}

public abstract record LuaMember
{
    public record ClassDocField(LuaDocFieldSyntax ClassDocFieldSyntax) : LuaMember;

    public record TableField(LuaTableFieldSyntax LocalTableFieldSyntax) : LuaMember;

    public record Index(LuaIndexExprSyntax IndexExprSyntax) : LuaMember;

    public record InterfaceDocField(LuaDocFieldSyntax Field) : LuaMember;

    public record Function(LuaFuncStatSyntax FuncStatSyntax) : LuaMember;
}

public abstract record LuaDocAttached
{
    public record Class(LuaSyntaxElement Attached) : LuaDocAttached;

    public record Interface(LuaSyntaxElement Attached) : LuaDocAttached;

    public record Enum(LuaSyntaxElement Attached) : LuaDocAttached;

    public record Type(LuaSyntaxElement Attached) : LuaDocAttached;
}
