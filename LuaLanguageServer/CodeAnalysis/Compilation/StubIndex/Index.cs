using LuaLanguageServer.CodeAnalysis.Compilation.Symbol;
using LuaLanguageServer.CodeAnalysis.Compilation.Type;
using LuaLanguageServer.CodeAnalysis.Syntax.Node;
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

    private static void AddFieldSymbol(
        StubIndexImpl stubIndexImpl, DocumentId documentId,
        IndexKey key, LuaSyntaxElement field,
        ILuaType fieldType, ILuaType containingType)
    {
        switch (key)
        {
            case IndexKey.Ty ty:
                // TODO add function
                break;
            case IndexKey.String str:
            {
                var fieldSymbol = new FieldSymbol(
                    field,
                    str.Value,
                    fieldType,
                    containingType
                );
                stubIndexImpl.ShortNameIndex.AddStub(documentId, str.Value, fieldSymbol);
                stubIndexImpl.Members.AddStub(documentId, containingType, fieldSymbol);
                break;
            }
            case IndexKey.Integer i:
            {
                var fieldSymbol = new FieldSymbol(
                    field,
                    i.Value,
                    fieldType,
                    containingType
                );
                stubIndexImpl.ShortNameIndex.AddStub(documentId, $"[{i.Value}]", fieldSymbol);
                stubIndexImpl.Members.AddStub(documentId, containingType, fieldSymbol);
                break;
            }
        }
    }

    private static void ClassIndex(StubIndexImpl stubIndexImpl, DocumentId documentId,
        LuaDocClassSyntax luaDocClassSyntax)
    {
        if (luaDocClassSyntax.Name is { } name)
        {
            var classType = new LuaClass(name.RepresentText);
            var classSymbol = new NamedSymbol(
                luaDocClassSyntax,
                name.RepresentText,
                classType,
                stubIndexImpl.Compilation.Builtin.Global
            );
            stubIndexImpl.ShortNameIndex.AddStub(documentId, name.RepresentText, classSymbol);
            stubIndexImpl.Members.AddStub(documentId, stubIndexImpl.Compilation.Builtin.Global, classSymbol);

            if (luaDocClassSyntax.Body is not { } body) return;
            var context = stubIndexImpl.Compilation.SearchContext;
            foreach (var field in body.FieldList)
            {
                var indexKey = IndexKey.FromDocTypedField(field, context);
                var fieldType = context.Infer(field.Type);
                AddFieldSymbol(stubIndexImpl, documentId, indexKey, field, fieldType, classType);
            }
        }
    }

    private static void EnumIndex(StubIndexImpl stubIndexImpl, DocumentId documentId,
        LuaDocEnumSyntax luaDocEnumSyntax)
    {
        if (luaDocEnumSyntax.Name is { } name)
        {
            var enumType = new LuaEnum(name.RepresentText);
            var enumSymbol = new NamedSymbol(
                luaDocEnumSyntax,
                luaDocEnumSyntax.Name.RepresentText,
                enumType,
                stubIndexImpl.Compilation.Builtin.Global
            );
            stubIndexImpl.ShortNameIndex.AddStub(documentId, name.RepresentText, enumSymbol);
            stubIndexImpl.Members.AddStub(documentId, stubIndexImpl.Compilation.Builtin.Global, enumSymbol);

            foreach (var field in luaDocEnumSyntax.FieldList)
            {
                if (field is { Name: { } fieldName })
                {
                    var fieldSymbol = new FieldSymbol(
                        field,
                        fieldName.RepresentText,
                        stubIndexImpl.Compilation.Builtin.Number,
                        enumType
                    );

                    stubIndexImpl.ShortNameIndex.AddStub(documentId, fieldName.RepresentText, fieldSymbol);
                    stubIndexImpl.Members.AddStub(documentId, enumType, fieldSymbol);
                }
            }
        }
    }

    private static void InterfaceIndex(StubIndexImpl stubIndexImpl, DocumentId documentId,
        LuaDocInterfaceSyntax luaDocInterfaceSyntax)
    {
        if (luaDocInterfaceSyntax.Name is { } name)
        {
            var interfaceType = new LuaInterface(name.RepresentText);
            var interfaceSymbol = new NamedSymbol(
                luaDocInterfaceSyntax,
                name.RepresentText,
                interfaceType,
                stubIndexImpl.Compilation.Builtin.Global);

            stubIndexImpl.ShortNameIndex.AddStub(documentId, name.RepresentText, interfaceSymbol);
            stubIndexImpl.Members.AddStub(documentId, stubIndexImpl.Compilation.Builtin.Global, interfaceSymbol);

            if (luaDocInterfaceSyntax.Body is not { } body) return;
            var context = stubIndexImpl.Compilation.SearchContext;
            foreach (var field in body.FieldList)
            {
                var indexKey = IndexKey.FromDocTypedField(field, context);
                var fieldType = context.Infer(field.Type);
                AddFieldSymbol(stubIndexImpl, documentId, indexKey, field, fieldType, interfaceType);
            }
        }
    }

    private static void AliasIndex(StubIndexImpl stubIndexImpl, DocumentId documentId,
        LuaDocAliasSyntax luaDocAliasSyntax)
    {
        if (luaDocAliasSyntax.Name is { } name)
        {
            var context = stubIndexImpl.Compilation.SearchContext;
            var baseTy = context.Infer(luaDocAliasSyntax.Type);
            var aliasType = new LuaAlias(name.RepresentText, baseTy);
            var aliasSymbol = new NamedSymbol(
                luaDocAliasSyntax,
                name.RepresentText,
                aliasType,
                stubIndexImpl.Compilation.Builtin.Global
            );

            stubIndexImpl.ShortNameIndex.AddStub(documentId, name.RepresentText, aliasSymbol);
            stubIndexImpl.Members.AddStub(documentId, stubIndexImpl.Compilation.Builtin.Global, aliasSymbol);
        }
    }

    private static void FieldIndex(StubIndexImpl stubIndexImpl, DocumentId documentId,
        LuaDocFieldSyntax luaDocFieldSyntax)
    {
        var context = stubIndexImpl.Compilation.SearchContext;
        var indexKey = IndexKey.FromDocField(luaDocFieldSyntax, context);
        var fieldType = context.Infer(luaDocFieldSyntax.Type);
        ILuaType? classType = null;
        if (luaDocFieldSyntax.PrevOfType<LuaDocClassSyntax>() is { Name : { } className })
        {
            var classSymbol = stubIndexImpl.ShortNameIndex.Get<NamedSymbol>(className.RepresentText).FirstOrDefault();
            classType = classSymbol?.GetType(context);
        }
        else if (luaDocFieldSyntax.PrevOfType<LuaDocInterfaceSyntax>() is { Name : { } interfaceName })
        {
            var classSymbol = stubIndexImpl.ShortNameIndex.Get<NamedSymbol>(interfaceName.RepresentText)
                .FirstOrDefault();
            classType = classSymbol?.GetType(context);
        }

        if (classType is null)
        {
            return;
        }

        AddFieldSymbol(stubIndexImpl, documentId, indexKey, luaDocFieldSyntax, fieldType, classType);
    }

    private static void FunctionIndex(StubIndexImpl stubIndexImpl, DocumentId documentId,
        LuaFuncStatSyntax luaFuncStatSyntax)
    {
        // if (luaFuncStatSyntax is { IsLocal: true, LocalName.Name: { } name })
        // {
        //     stubIndexImpl.ShortNameIndex.AddStub(
        //         documentId, name.RepresentText, new LuaShortName.Function(luaFuncStatSyntax));
        // }

        // if (luaFuncStatSyntax is { PrefixExpr: { } prefixExpr })
        // {
        //     stubIndexImpl.Members.AddStub(
        //         documentId, prefixExpr, new LuaMember.Function(luaFuncStatSyntax));
        // }
        throw new NotImplementedException();
    }

    private static void LabelIndex(StubIndexImpl stubIndexImpl, DocumentId documentId,
        LuaLabelStatSyntax luaLabelStatSyntax)
    {
        if (luaLabelStatSyntax.Name is { } name)
        {
            var labelSymbol = new LabelSymbol(luaLabelStatSyntax, name.RepresentText);
            stubIndexImpl.ShortNameIndex.AddStub(documentId, name.RepresentText, labelSymbol);
        }
    }

    private static void ForIndex(StubIndexImpl stubIndexImpl, DocumentId documentId,
        LuaForStatSyntax luaForStatSyntax)
    {
        if (luaForStatSyntax.IteratorName is { Name: { } name } it)
        {
            var context = stubIndexImpl.Compilation.SearchContext;
            var localType = context.Infer(luaForStatSyntax.InitExpr);
            var localSymbol = new LocalSymbol(it, name.RepresentText, localType);
            stubIndexImpl.ShortNameIndex.AddStub(documentId, name.RepresentText, localSymbol);
        }
    }

    private static void ForRangeIndex(StubIndexImpl stubIndexImpl, DocumentId documentId,
        LuaForRangeStatSyntax luaForRangeStatSyntax)
    {
        foreach (var iteratorName in luaForRangeStatSyntax.IteratorNames)
        {
            if (iteratorName is { Name: { } name })
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

            if (localName is { Name: { } name })
            {
                stubIndexImpl.ShortNameIndex.AddStub(
                    documentId, name.RepresentText,
                    lastValidExprId == -1
                        ? new LuaShortName.Local(localName, null, -1)
                        : new LuaShortName.Local(localName, lastValidExpr, i - lastValidExprId));
            }

            if (i == 0)
            {
                AttachedIndex(stubIndexImpl, documentId, localName);
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

            switch (varName)
            {
                case LuaNameExprSyntax { Name: { } exprName }:
                    stubIndexImpl.ShortNameIndex.AddStub(
                        documentId, exprName.RepresentText,
                        lastValidExprId == -1
                            ? new LuaShortName.VarDef(varName, null, -1)
                            : new LuaShortName.VarDef(varName, lastValidExpr, i - lastValidExprId));
                    break;
                case LuaIndexExprSyntax { DotOrColonIndexName: { } name }:
                {
                    stubIndexImpl.ShortNameIndex.AddStub(
                        documentId, name.RepresentText,
                        lastValidExprId == -1
                            ? new LuaShortName.VarDef(varName, null, -1)
                            : new LuaShortName.VarDef(varName, lastValidExpr, i - lastValidExprId));
                    break;
                }
                case LuaIndexExprSyntax
                {
                    IsKeyIndex: true, IndexKeyExpr: LuaLiteralExprSyntax { Literal: LuaStringToken stringToken }
                }:
                {
                    stubIndexImpl.ShortNameIndex.AddStub(
                        documentId, stringToken.InnerString,
                        lastValidExprId == -1
                            ? new LuaShortName.VarDef(varName, null, -1)
                            : new LuaShortName.VarDef(varName, lastValidExpr, i - lastValidExprId));

                    break;
                }
            }

            if (i == 0)
            {
                AttachedIndex(stubIndexImpl, documentId, varName);
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
            case { NameKey: { } nameKey }:
            {
                stubIndexImpl.ShortNameIndex.AddStub(
                    documentId, nameKey.RepresentText, new LuaShortName.TableField(luaTableFieldSyntax));
                break;
            }
            case { StringKey: { } stringKey }:
            {
                stubIndexImpl.ShortNameIndex.AddStub(
                    documentId, stringKey.InnerString, new LuaShortName.TableField(luaTableFieldSyntax));
                break;
            }
            case { NumberKey: { } numberKey }:
            {
                if (numberKey is LuaIntegerToken integerToken)
                {
                    stubIndexImpl.ShortNameIndex.AddStub(
                        documentId, $"[{integerToken.Value}]", new LuaShortName.TableField(luaTableFieldSyntax));
                    break;
                }
                else
                {
                    stubIndexImpl.ShortNameIndex.AddStub(
                        documentId, $"[{numberKey.RepresentText}]", new LuaShortName.TableField(luaTableFieldSyntax));
                    break;
                }
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
            if (genericParam is { Name: { } name })
            {
                stubIndexImpl.ShortNameIndex.AddStub(
                    documentId, name.RepresentText, new LuaShortName.Generic(genericParam));
            }
        }
    }

    private static void AttachedIndex(StubIndexImpl stubIndexImpl, DocumentId documentId,
        LuaSyntaxElement attached)
    {
        var stat = attached.AncestorsAndSelf.OfType<LuaStatSyntax>().FirstOrDefault();
        if (stat?.Comments.FirstOrDefault() is { } comment)
        {
            var doc = comment.DocList.FirstOrDefault(it => it is LuaDocClassSyntax
                or LuaDocInterfaceSyntax or LuaDocEnumSyntax
            );
            switch (doc)
            {
                case LuaDocClassSyntax classSyntax:
                    stubIndexImpl.Attached.AddStub(documentId, classSyntax, new LuaDocAttached.Class(attached));
                    break;
                case LuaDocInterfaceSyntax interfaceSyntax:
                    stubIndexImpl.Attached.AddStub(
                        documentId, interfaceSyntax, new LuaDocAttached.Interface(attached));
                    break;
                case LuaDocEnumSyntax enumSyntax:
                    stubIndexImpl.Attached.AddStub(documentId, enumSyntax, new LuaDocAttached.Enum(attached));
                    break;
            }
        }
    }

    public static void RemoveIndex(StubIndexImpl stubIndexImpl, DocumentId documentId, LuaSyntaxTree tree)
    {
        stubIndexImpl.ShortNameIndex.RemoveStub(documentId);
        stubIndexImpl.Members.RemoveStub(documentId);
        // stubIndexImpl.Attached.RemoveStub(documentId);
    }
}
