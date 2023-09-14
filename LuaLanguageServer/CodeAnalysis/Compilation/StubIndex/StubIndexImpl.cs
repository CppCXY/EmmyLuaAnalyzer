using LuaLanguageServer.CodeAnalysis.Syntax.Node;
using LuaLanguageServer.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace LuaLanguageServer.CodeAnalysis.Compilation.StubIndex;

public class StubIndexImpl
{
    public StubIndex<int, LuaSyntaxNode> ClassMemberIndex { get; set; } = new();
    public StubIndex<string, LuaDocClassSyntax> ClassIndex { get; set; } = new();
    public StubIndex<string, LuaDocEnumSyntax> EnumIndex { get; set; } = new();
    public StubIndex<string, LuaDocAliasSyntax> AliasIndex { get; set; } = new();

    public StubIndex<string, LuaDocInterfaceSyntax> InterfaceIndex { get; set; } = new();

    public StubIndex<LuaDocClassSyntax, LuaDocFieldSyntax> ClassField { get; set; } = new();
}
