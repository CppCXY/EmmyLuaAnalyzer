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
                    stubIndexImpl.ShortNameIndex.AddStub(
                        documentId, name.RepresentText, new LuaShortName.Class(luaDocClassSyntax));
                    break;
                }
                case LuaDocEnumSyntax { Name: { } name } luaDocEnumSyntax:
                {
                    stubIndexImpl.ShortNameIndex.AddStub(
                        documentId, name.RepresentText, new LuaShortName.Enum(luaDocEnumSyntax));

                    foreach (var fieldSyntax in luaDocEnumSyntax.FieldList)
                    {
                        if (fieldSyntax is { Name: { } fieldName })
                        {
                            stubIndexImpl.ShortNameIndex.AddStub(
                                documentId, fieldName.RepresentText, new LuaShortName.EnumField(fieldSyntax));
                        }
                    }

                    break;
                }
                case LuaDocAliasSyntax { Name: { } name } luaDocAliasSyntax:
                {
                    stubIndexImpl.ShortNameIndex.AddStub(
                        documentId, name.RepresentText, new LuaShortName.Alias(luaDocAliasSyntax));
                    break;
                }
                case LuaDocInterfaceSyntax { Name: { } name } luaDocInterfaceSyntax:
                {
                    stubIndexImpl.ShortNameIndex.AddStub(
                        documentId, name.RepresentText, new LuaShortName.Interface(luaDocInterfaceSyntax));
                    break;
                }
                case LuaDocFieldSyntax luaDocFieldSyntax:
                {
                    switch (luaDocFieldSyntax)
                    {
                        case { NameField: { } nameField }:
                        {
                            stubIndexImpl.ShortNameIndex.AddStub(
                                documentId, nameField.RepresentText, new LuaShortName.Field(luaDocFieldSyntax));
                            break;
                        }
                        case { IntegerField: { } integerField }:
                        {
                            stubIndexImpl.ShortNameIndex.AddStub(
                                documentId, integerField.RepresentText, new LuaShortName.Field(luaDocFieldSyntax));
                            break;
                        }
                        case { StringField: { } stringField }:
                        {
                            stubIndexImpl.ShortNameIndex.AddStub(
                                documentId, stringField.RepresentText, new LuaShortName.Field(luaDocFieldSyntax));
                            break;
                        }
                    }

                    if (luaDocFieldSyntax.PrevOfType<LuaDocClassSyntax>() is { } luaDocClassSyntax)
                    {
                        stubIndexImpl.Members.AddStub(
                            documentId, luaDocClassSyntax, new LuaMember.ClassDocField(luaDocFieldSyntax));
                    }

                    break;
                }
                case LuaFuncStatSyntax luaFuncStatSyntax:
                {
                    if (luaFuncStatSyntax.Name is { } name)
                    {
                        stubIndexImpl.ShortNameIndex.AddStub(
                            documentId, name.RepresentText, new LuaShortName.Function(luaFuncStatSyntax));
                    }

                    break;
                }
            }
        }
    }

    public static void RemoveIndex(StubIndexImpl stubIndexImpl, DocumentId documentId, LuaSyntaxTree tree)
    {
        stubIndexImpl.ShortNameIndex.RemoveStub(documentId);
        stubIndexImpl.Members.RemoveStub(documentId);
    }
}
