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
                case LuaDocClassSyntax { Name: { } name } luaDocClassSyntax:
                {
                    var nameText = name.Text.ToString();
                    stubIndexImpl.ClassIndex.AddStub(documentId, nameText, luaDocClassSyntax);
                    break;
                }
                case LuaDocEnumSyntax { Name: { } name } luaDocEnumSyntax:
                {
                    var nameText = name.Text.ToString();
                    stubIndexImpl.EnumIndex.AddStub(documentId, nameText, luaDocEnumSyntax);
                    break;
                }
                case LuaDocAliasSyntax { Name: { } name } luaDocAliasSyntax:
                {
                    var nameText = name.Text.ToString();
                    stubIndexImpl.AliasIndex.AddStub(documentId, nameText, luaDocAliasSyntax);
                    break;
                }
                case LuaDocInterfaceSyntax { Name: { } name } luaDocInterfaceSyntax:
                {
                    var nameText = name.Text.ToString();
                    stubIndexImpl.InterfaceIndex.AddStub(documentId, nameText, luaDocInterfaceSyntax);
                    break;
                }
                case LuaDocFieldSyntax luaDocFieldSyntax:
                {
                    if (luaDocFieldSyntax.PrevOfType<LuaDocClassSyntax>() is { } luaDocClassSyntax)
                    {
                        stubIndexImpl.ClassField.AddStub(documentId, luaDocClassSyntax, luaDocFieldSyntax);
                    }

                    break;
                }
            }
        }
    }

    public static void RemoveIndex(StubIndexImpl stubIndexImpl, DocumentId documentId, LuaSyntaxTree tree)
    {
        stubIndexImpl.ClassIndex.RemoveStub(documentId);
        stubIndexImpl.AliasIndex.RemoveStub(documentId);
        stubIndexImpl.EnumIndex.RemoveStub(documentId);
        stubIndexImpl.InterfaceIndex.RemoveStub(documentId);
        stubIndexImpl.ClassField.RemoveStub(documentId);
    }
}
