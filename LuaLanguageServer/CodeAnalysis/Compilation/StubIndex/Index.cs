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
                    ClassIndex(stubIndexImpl, documentId, luaDocClassSyntax);
                    break;
                }
                case LuaDocEnumSyntax luaDocEnumSyntax:
                {
                    EnumIndex(stubIndexImpl, documentId, luaDocEnumSyntax);
                    break;
                }
                case LuaDocAliasSyntax luaDocAliasSyntax:
                {
                    AliasIndex(stubIndexImpl, documentId, luaDocAliasSyntax);
                    break;
                }
                case LuaDocInterfaceSyntax luaDocInterfaceSyntax:
                {
                    InterfaceIndex(stubIndexImpl, documentId, luaDocInterfaceSyntax);
                    break;
                }
                case LuaDocFieldSyntax luaDocFieldSyntax:
                {
                    FieldIndex(stubIndexImpl, documentId, luaDocFieldSyntax);
                    break;
                }
                case LuaFuncStatSyntax luaFuncStatSyntax:
                {
                    FunctionIndex(stubIndexImpl, documentId, luaFuncStatSyntax);
                    break;
                }
            }
        }
    }

    private static void ClassIndex(StubIndexImpl stubIndexImpl, DocumentId documentId,
        LuaDocClassSyntax luaDocClassSyntax)
    {
        if (luaDocClassSyntax.Name is { } name)
        {
            stubIndexImpl.ShortNameIndex.AddStub(
                documentId, name.RepresentText, new LuaShortName.Class(luaDocClassSyntax));
        }
    }

    private static void EnumIndex(StubIndexImpl stubIndexImpl, DocumentId documentId,
        LuaDocEnumSyntax luaDocEnumSyntax)
    {
        if (luaDocEnumSyntax.Name is { } name)
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
        }
    }

    private static void InterfaceIndex(StubIndexImpl stubIndexImpl, DocumentId documentId,
        LuaDocInterfaceSyntax luaDocInterfaceSyntax)
    {
        if (luaDocInterfaceSyntax.Name is { } name)
        {
            stubIndexImpl.ShortNameIndex.AddStub(
                documentId, name.RepresentText, new LuaShortName.Interface(luaDocInterfaceSyntax));
        }
    }

    private static void AliasIndex(StubIndexImpl stubIndexImpl, DocumentId documentId,
        LuaDocAliasSyntax luaDocAliasSyntax)
    {
        if (luaDocAliasSyntax.Name is { } name)
        {
            stubIndexImpl.ShortNameIndex.AddStub(
                documentId, name.RepresentText, new LuaShortName.Alias(luaDocAliasSyntax));
        }
    }

    private static void FieldIndex(StubIndexImpl stubIndexImpl, DocumentId documentId,
        LuaDocFieldSyntax luaDocFieldSyntax)
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
    }

    private static void FunctionIndex(StubIndexImpl stubIndexImpl, DocumentId documentId,
        LuaFuncStatSyntax luaFuncStatSyntax)
    {
        if (luaFuncStatSyntax is { IsMethod: false, Name: { } name })
        {
            stubIndexImpl.ShortNameIndex.AddStub(
                documentId, name.RepresentText, new LuaShortName.Function(luaFuncStatSyntax));
        }
        else
        {
            // foreach (var methodName in luaFuncStatSyntax.MethodNames)
            // {
            //     stubIndexImpl.ShortNameIndex.AddStub(
            //         documentId, methodName.Name.RepresentText,
            //         new LuaShortName.Function(luaFuncStatSyntax));
            // }
        }
    }

    public static void RemoveIndex(StubIndexImpl stubIndexImpl, DocumentId documentId, LuaSyntaxTree tree)
    {
        stubIndexImpl.ShortNameIndex.RemoveStub(documentId);
        stubIndexImpl.Members.RemoveStub(documentId);
    }
}
