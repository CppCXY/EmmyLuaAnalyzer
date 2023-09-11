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
            // switch (node)
            // {
            //     case LuaDocClassSyntax luaDocClassSyntax:
            //         stubIndexImpl.ClassIndex.AddStub(documentId, luaDocClassSyntax.Name.Text, luaDocClassSyntax);
            //         break;
            //     case LuaDocEnumSyntax luaDocEnumSyntax:
            //         stubIndexImpl.EnumIndex.AddStub(documentId, luaDocEnumSyntax.Name.Text, luaDocEnumSyntax);
            //         break;
            //     case LuaDocAliasSyntax luaDocAliasSyntax:
            //         stubIndexImpl.AliasIndex.AddStub(documentId, luaDocAliasSyntax.Name.Text, luaDocAliasSyntax);
            //         break;
            //     case LuaDocTypeSyntax luaDocTypeSyntax:
            //         stubIndexImpl.SuperIndex.AddStub(documentId, luaDocTypeSyntax.Name.Text, luaDocTypeSyntax);
            //         break;
            //     case LuaDocClassMemberSyntax luaDocClassMemberSyntax:
            //         stubIndexImpl.ClassMemberIndex.AddStub(documentId, luaDocClassMemberSyntax.Name.Text, luaDocClassMemberSyntax);
            //         break;
            // }
        }
    }

    public static void RemoveIndex(StubIndexImpl stubIndexImpl, DocumentId documentId, LuaSyntaxTree tree)
    {
    }
}
