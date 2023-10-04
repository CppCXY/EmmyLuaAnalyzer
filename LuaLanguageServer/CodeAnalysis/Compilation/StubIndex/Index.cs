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
                case LuaAssignStatSyntax luaAssignStatSyntax:
                {
                    AssignIndex(stubIndexImpl, documentId, luaAssignStatSyntax);
                    break;
                }
                case LuaIndexExprSyntax luaIndexExprSyntax:
                {
                    IndexExprIndex(stubIndexImpl, documentId, luaIndexExprSyntax);
                    break;
                }
                case LuaTableFieldSyntax luaTableFieldSyntax:
                {
                    TableFieldIndex(stubIndexImpl, documentId, luaTableFieldSyntax);
                    break;
                }
                case LuaParamListSyntax luaParamListSyntax:
                {
                    ParamIndex(stubIndexImpl, documentId, luaParamListSyntax);
                    break;
                }
                case LuaDocGenericSyntax luaDocGenericSyntax:
                {
                    GenericDocIndex(stubIndexImpl, documentId, luaDocGenericSyntax);
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
                if (fieldSyntax is {Name: { } fieldName})
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
            case {NameField: { } nameField}:
            {
                stubIndexImpl.ShortNameIndex.AddStub(
                    documentId, nameField.RepresentText, new LuaShortName.Field(luaDocFieldSyntax));
                break;
            }
            case {IntegerField: { } integerField}:
            {
                stubIndexImpl.ShortNameIndex.AddStub(
                    documentId, integerField.RepresentText, new LuaShortName.Field(luaDocFieldSyntax));
                break;
            }
            case {StringField: { } stringField}:
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
            case {IsMethod: false, Name: { } name}:
            {
                stubIndexImpl.ShortNameIndex.AddStub(
                    documentId, name.RepresentText, new LuaShortName.Function(luaFuncStatSyntax));
                break;
            }
            case {IsMethod: true, Name: { } name2}:
            {
                stubIndexImpl.ShortNameIndex.AddStub(
                    documentId, name2.RepresentText, new LuaShortName.Function(luaFuncStatSyntax));

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
        if (luaForStatSyntax.IteratorName is {Name: { } name})
        {
            stubIndexImpl.ShortNameIndex.AddStub(
                documentId, name.RepresentText, new LuaShortName.Param(luaForStatSyntax.IteratorName));
        }
    }

    private static void ForRangeIndex(StubIndexImpl stubIndexImpl, DocumentId documentId,
        LuaForRangeStatSyntax luaForRangeStatSyntax)
    {
        foreach (var iteratorName in luaForRangeStatSyntax.IteratorNames)
        {
            if (iteratorName is {Name: { } name})
            {
                stubIndexImpl.ShortNameIndex.AddStub(
                    documentId, name.RepresentText, new LuaShortName.Param(iteratorName));
            }
        }
    }

    private static void LocalIndex(StubIndexImpl stubIndexImpl, DocumentId documentId,
        LuaLocalStatSyntax luaLocalStatSyntax)
    {
        var count = luaLocalStatSyntax.NameList.Count();
        var lastValidExprId = -1;
        LuaExprSyntax? lastValidExpr = null;
        for (var i = 0; i < count; i++)
        {
            var localName = luaLocalStatSyntax.NameList.ElementAt(i);
            var expr = luaLocalStatSyntax.ExpressionList.ElementAtOrDefault(i);
            if (expr is not null)
            {
                lastValidExpr = expr;
                lastValidExprId = i;
            }

            if (localName is {Name: { } name})
            {
                stubIndexImpl.ShortNameIndex.AddStub(
                    documentId, name.RepresentText,
                    lastValidExprId == -1
                        ? new LuaShortName.Local(localName, null, -1)
                        : new LuaShortName.Local(localName, lastValidExpr, i - lastValidExprId));
            }
        }
    }

    private static void AssignIndex(StubIndexImpl stubIndexImpl, DocumentId documentId,
        LuaAssignStatSyntax luaAssignStatSyntax)
    {
        var count = luaAssignStatSyntax.VarList.Count();
        var lastValidExprId = -1;
        LuaExprSyntax? lastValidExpr = null;
        for (var i = 0; i < count; i++)
        {
            var varName = luaAssignStatSyntax.VarList.ElementAt(i);
            var expr = luaAssignStatSyntax.ExprList.ElementAtOrDefault(i);
            if (expr is not null)
            {
                lastValidExpr = expr;
                lastValidExprId = i;
            }

            if (varName is LuaNameExprSyntax nameExpr)
            {
                stubIndexImpl.ShortNameIndex.AddStub(
                    documentId, nameExpr.Name.RepresentText,
                    lastValidExprId == -1
                        ? new LuaShortName.VarDef(varName, null, -1)
                        : new LuaShortName.VarDef(varName, lastValidExpr, i - lastValidExprId));
            }
            else if (varName is LuaIndexExprSyntax {Name : { } name} indexExpr)
            {
                stubIndexImpl.ShortNameIndex.AddStub(
                    documentId, name.RepresentText,
                    lastValidExprId == -1
                        ? new LuaShortName.VarDef(varName, null, -1)
                        : new LuaShortName.VarDef(varName, lastValidExpr, i - lastValidExprId));
            }
        }
    }

    private static void IndexExprIndex(StubIndexImpl stubIndexImpl, DocumentId documentId,
        LuaIndexExprSyntax luaIndexExprSyntax)
    {
        if (luaIndexExprSyntax.PrefixExpr is { } prefixExpr)
        {
            stubIndexImpl.Members.AddStub(
                documentId, prefixExpr, new LuaMember.Index(luaIndexExprSyntax));
        }
    }

    private static void TableFieldIndex(StubIndexImpl stubIndexImpl, DocumentId documentId,
        LuaTableFieldSyntax luaTableFieldSyntax)
    {
        switch (luaTableFieldSyntax)
        {
            case {NameKey: { } nameKey}:
            {
                stubIndexImpl.ShortNameIndex.AddStub(
                    documentId, nameKey.RepresentText, new LuaShortName.TableField(luaTableFieldSyntax));
                break;
            }
            case {StringKey: { } stringKey}:
            {
                stubIndexImpl.ShortNameIndex.AddStub(
                    documentId, stringKey.RepresentText, new LuaShortName.TableField(luaTableFieldSyntax));
                break;
            }
            case {NumberKey: { } numberKey}:
            {
                stubIndexImpl.ShortNameIndex.AddStub(
                    documentId, numberKey.RepresentText, new LuaShortName.TableField(luaTableFieldSyntax));
                break;
            }
        }

        if (luaTableFieldSyntax.Parent is { } parentExpr)
        {
            stubIndexImpl.Members.AddStub(
                documentId, parentExpr, new LuaMember.TableField(luaTableFieldSyntax));
        }
    }

    private static void ParamIndex(StubIndexImpl stubIndexImpl, DocumentId documentId,
        LuaParamListSyntax luaParamListSyntax)
    {
        foreach (var param in luaParamListSyntax.Params)
        {
            if (param.Name is { } name)
            {
                stubIndexImpl.ShortNameIndex.AddStub(
                    documentId, name.RepresentText, new LuaShortName.Param(param));
            }
        }
    }

    private static void GenericDocIndex(StubIndexImpl stubIndexImpl, DocumentId documentId,
        LuaDocGenericSyntax luaDocGenericSyntax)
    {
        foreach (var genericParam in luaDocGenericSyntax.ParamList)
        {
            if (genericParam is {Name: { } name})
            {
                stubIndexImpl.ShortNameIndex.AddStub(
                    documentId, name.RepresentText, new LuaShortName.Generic(genericParam));
            }
        }
    }

    public static void RemoveIndex(StubIndexImpl stubIndexImpl, DocumentId documentId, LuaSyntaxTree tree)
    {
        stubIndexImpl.ShortNameIndex.RemoveStub(documentId);
        stubIndexImpl.Members.RemoveStub(documentId);
    }
}
