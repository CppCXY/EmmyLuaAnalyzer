using LuaLanguageServer.CodeAnalysis.Syntax.Node;
using LuaLanguageServer.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace LuaLanguageServer.CodeAnalysis.Compilation.StubIndex;

public class StubIndexImpl
{
    public StubIndex<string, LuaShortName> ShortNameIndex { get; set; } = new();

    public StubIndex<LuaSyntaxNode, LuaMember> Members { get; set; } = new();
}

public abstract record LuaShortName
{
    public record Class(LuaDocClassSyntax ClassSyntax) : LuaShortName;

    public record Enum(LuaDocEnumSyntax EnumSyntax) : LuaShortName;

    public record Alias(LuaDocAliasSyntax AliasSyntax) : LuaShortName;

    public record Interface(LuaDocInterfaceSyntax InterfaceSyntax) : LuaShortName;

    public record Field(LuaDocFieldSyntax FieldSyntax) : LuaShortName;

    public record EnumField(LuaDocEnumFieldSyntax EnumFieldSyntax) : LuaShortName;

    public record Local(LuaSyntaxToken LocalSName) : LuaShortName;

    public record Param(LuaSyntaxToken ParamName) : LuaShortName;

    public record TableFieldIndex(LuaTableFieldSyntax TableFieldSyntax) : LuaShortName;

    public record Label(LuaLabelStatSyntax LabelStatSyntax) : LuaShortName;

    public record Function(LuaFuncStatSyntax FuncStatSyntax) : LuaShortName;
}

public abstract record LuaMember
{
    public record ClassDocField(LuaDocFieldSyntax ClassDocFieldSyntax) : LuaMember;

    public record ClassTableField(LuaTableFieldSyntax ClassTableFieldSyntax) : LuaMember;

    public record EnumDocField(LuaDocEnumFieldSyntax EnumDocFieldSyntax) : LuaMember;

    public record EnumTableField(LuaTableFieldSyntax EnumTableFieldSyntax) : LuaMember;

    public record InterfaceField(LuaDocFieldSyntax FieldSyntax) : LuaMember;

    public record TableField(LuaTableFieldSyntax LocalTableFieldSyntax) : LuaMember;
}
