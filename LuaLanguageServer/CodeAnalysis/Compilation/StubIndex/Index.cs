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
                case LuaLabelStatSyntax luaLabelStatSyntax:
                {
                    LabelIndex(stubIndexImpl, documentId, luaLabelStatSyntax);
                    break;
                }
                case LuaGotoStatSyntax luaGotoStatSyntax:
                {
                    GotoIndex(stubIndexImpl, documentId, luaGotoStatSyntax);
                    break;
                }
                case LuaForStatSyntax luaForStatSyntax:
                {
                    ForIndex(stubIndexImpl, documentId, luaForStatSyntax);
                    break;
                }
                case LuaForRangeStatSyntax luaForRangeStatSyntax:
                {
                    ForRangeIndex(stubIndexImpl, documentId, luaForRangeStatSyntax);
                    break;
                }
                case LuaLocalStatSyntax luaLocalStatSyntax:
                {
                    LocalIndex(stubIndexImpl, documentId, luaLocalStatSyntax);
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
        switch (luaFuncStatSyntax)
        {
            case { IsMethod: false, Name: { } name }:
            {
                stubIndexImpl.ShortNameIndex.AddStub(
                    documentId, name.RepresentText, new LuaShortName.Function(luaFuncStatSyntax));
                break;
            }
            case { IsMethod: true, Name: { } name2, ParentExpr: { } parentExpr }:
            {
                stubIndexImpl.ShortNameIndex.AddStub(
                    documentId, name2.RepresentText, new LuaShortName.Function(luaFuncStatSyntax));

                stubIndexImpl.Members.AddStub(
                    documentId, parentExpr, new LuaMember.Function(luaFuncStatSyntax));

                while (parentExpr is LuaIndexExprSyntax indexExpr)
                {
                    parentExpr = indexExpr.ParentExpr;
                    if (parentExpr is not null)
                    {
                        stubIndexImpl.Members.AddStub(
                            documentId, parentExpr, new LuaMember.Index(indexExpr));
                    }
                }

                break;
            }
        }
    }

    private static void LabelIndex(StubIndexImpl stubIndexImpl, DocumentId documentId,
        LuaLabelStatSyntax luaLabelStatSyntax)
    {
        if (luaLabelStatSyntax.Name is { } name)
        {
            stubIndexImpl.ShortNameIndex.AddStub(
                documentId, name.RepresentText, new LuaShortName.Label(luaLabelStatSyntax));
        }
    }

    private static void GotoIndex(StubIndexImpl stubIndexImpl, DocumentId documentId,
        LuaGotoStatSyntax luaGotoStatSyntax)
    {
        if (luaGotoStatSyntax.LabelName is { } name)
        {
            stubIndexImpl.ShortNameIndex.AddStub(
                documentId, name.RepresentText, new LuaShortName.Goto(luaGotoStatSyntax));
        }
    }

    private static void ForIndex(StubIndexImpl stubIndexImpl, DocumentId documentId,
        LuaForStatSyntax luaForStatSyntax)
    {
        if (luaForStatSyntax.IteratorName is { } name)
        {
            stubIndexImpl.ShortNameIndex.AddStub(
                documentId, name.RepresentText, new LuaShortName.Param(name));
        }
    }

    private static void ForRangeIndex(StubIndexImpl stubIndexImpl, DocumentId documentId,
        LuaForRangeStatSyntax luaForRangeStatSyntax)
    {
        foreach (var name in luaForRangeStatSyntax.IteratorNames)
        {
            stubIndexImpl.ShortNameIndex.AddStub(
                documentId, name.RepresentText, new LuaShortName.Param(name));
        }
    }

    private static void LocalIndex(StubIndexImpl stubIndexImpl, DocumentId documentId,
        LuaLocalStatSyntax luaLocalStatSyntax)
    {
        foreach (var (localName, expr) in
                 luaLocalStatSyntax.NameList.Zip(luaLocalStatSyntax.ExpressionList,
                     (n, e) => (n, e)
                 ))
        {
            if (localName is { Name: { } name })
            {
                stubIndexImpl.ShortNameIndex.AddStub(
                    documentId, name.RepresentText, new LuaShortName.Local(localName, expr));
            }
        }
    }

    public static void RemoveIndex(StubIndexImpl stubIndexImpl, DocumentId documentId, LuaSyntaxTree tree)
    {
        stubIndexImpl.ShortNameIndex.RemoveStub(documentId);
        stubIndexImpl.Members.RemoveStub(documentId);
    }
}
