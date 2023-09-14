using LuaLanguageServer.CodeAnalysis.Syntax.Node.SyntaxNodes;
using LuaLanguageServer.CodeAnalysis.Syntax.Tree;
using LuaLanguageServer.CodeAnalysis.Workspace;

namespace LuaLanguageServer.CodeAnalysis.Compilation.StubIndex;

public static class Index
{
    public static void BuildIndex(StubIndexImpl stubIndexImpl, DocumentId documentId, LuaSyntaxTree tree)
    {
        foreach (var node in tree.SyntaxRoot.Descendants)
        {
            switch (node)
            {
                case LuaDocClassSyntax luaDocClassSyntax:
                {
                    if (luaDocClassSyntax.Name == null)
                    {
                        break;
                    }

                    var name = luaDocClassSyntax.Name.Text.ToString();
                    stubIndexImpl.ClassIndex.AddStub(documentId, name, luaDocClassSyntax);
                    break;
                }
                case LuaDocEnumSyntax luaDocEnumSyntax:
                {
                    if (luaDocEnumSyntax.Name == null)
                    {
                        break;
                    }

                    var name = luaDocEnumSyntax.Name.Text.ToString();
                    stubIndexImpl.EnumIndex.AddStub(documentId, name, luaDocEnumSyntax);
                    break;
                }
                case LuaDocAliasSyntax luaDocAliasSyntax:
                {
                    if (luaDocAliasSyntax.Name == null)
                    {
                        break;
                    }

                    var name = luaDocAliasSyntax.Name.Text.ToString();
                    stubIndexImpl.AliasIndex.AddStub(documentId, name, luaDocAliasSyntax);
                    break;
                }
                case LuaDocInterfaceSyntax luaDocInterfaceSyntax:
                {
                    if (luaDocInterfaceSyntax.Name == null)
                    {
                        break;
                    }

                    var name = luaDocInterfaceSyntax.Name.Text.ToString();
                    stubIndexImpl.InterfaceIndex.AddStub(documentId, name, luaDocInterfaceSyntax);
                    break;
                }
            }
        }
    }

    public static void RemoveIndex(StubIndexImpl stubIndexImpl, DocumentId documentId, LuaSyntaxTree tree)
    {
    }
}
