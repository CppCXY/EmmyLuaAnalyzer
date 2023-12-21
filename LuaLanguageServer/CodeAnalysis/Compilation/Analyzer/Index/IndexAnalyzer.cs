using LuaLanguageServer.CodeAnalysis.Compilation.Analyzer.Stub;
using LuaLanguageServer.CodeAnalysis.Compilation.Symbol;
using LuaLanguageServer.CodeAnalysis.Compilation.Type;
using LuaLanguageServer.CodeAnalysis.Syntax.Node;
using LuaLanguageServer.CodeAnalysis.Syntax.Node.SyntaxNodes;
using LuaLanguageServer.CodeAnalysis.Workspace;

namespace LuaLanguageServer.CodeAnalysis.Compilation.Analyzer.Index;

public class IndexAnalyzer(LuaCompilation compilation) : LuaAnalyzer(compilation)
{
    // public override void Analyze(DocumentId documentId)
    // {
    //     if (Compilation.GetSyntaxTree(documentId) is { } tree)
    //     {
    //         foreach (var node in tree.SyntaxRoot.Descendants)
    //         {
    //             switch (node)
    //             {
    //                 case LuaDocTagClassSyntax luaDocClassSyntax:
    //                 {
    //                     ClassIndex(documentId, luaDocClassSyntax);
    //                     break;
    //                 }
    //                 case LuaDocTagEnumSyntax luaDocEnumSyntax:
    //                 {
    //                     EnumIndex(documentId, luaDocEnumSyntax);
    //                     break;
    //                 }
    //                 case LuaDocTagAliasSyntax luaDocAliasSyntax:
    //                 {
    //                     AliasIndex(documentId, luaDocAliasSyntax);
    //                     break;
    //                 }
    //                 case LuaDocTagInterfaceSyntax luaDocInterfaceSyntax:
    //                 {
    //                     InterfaceIndex(documentId, luaDocInterfaceSyntax);
    //                     break;
    //                 }
    //                 case LuaDocTagFieldSyntax luaDocFieldSyntax:
    //                 {
    //                     FieldIndex(documentId, luaDocFieldSyntax);
    //                     break;
    //                 }
    //                 case LuaFuncStatSyntax luaFuncStatSyntax:
    //                 {
    //                     MethodIndex(documentId, luaFuncStatSyntax);
    //                     break;
    //                 }
    //                 case LuaForStatSyntax luaForStatSyntax:
    //                 {
    //                     ForIndex(documentId, luaForStatSyntax);
    //                     break;
    //                 }
    //                 case LuaForRangeStatSyntax luaForRangeStatSyntax:
    //                 {
    //                     ForRangeIndex(documentId, luaForRangeStatSyntax);
    //                     break;
    //                 }
    //                 case LuaLocalStatSyntax luaLocalStatSyntax:
    //                 {
    //                     LocalIndex(documentId, luaLocalStatSyntax);
    //                     break;
    //                 }
    //                 case LuaAssignStatSyntax luaAssignStatSyntax:
    //                 {
    //                     AssignIndex(documentId, luaAssignStatSyntax);
    //                     break;
    //                 }
    //                 case LuaTableExprSyntax luaTableSyntax:
    //                 {
    //                     TableIndex(documentId, luaTableSyntax);
    //                     break;
    //                 }
    //             }
    //         }
    //     }
    // }
    //
    //
    // private void AddFieldSymbol(
    //     DocumentId documentId,
    //     IndexKey key, LuaSyntaxElement field,
    //     LuaSyntaxElement? fieldType, ILuaType containingType)
    // {
    //     var stubIndexImpl = Compilation.StubIndexImpl;
    //     switch (key)
    //     {
    //         case IndexKey.Ty ty:
    //             // TODO add function
    //             break;
    //         case IndexKey.String str:
    //         {
    //             var fieldSymbol = new NamedSymbol(
    //                 field,
    //                 str.Value,
    //                 new LuaLazyType(fieldType),
    //                 containingType
    //             );
    //             stubIndexImpl.ShortNameIndex.AddStub(documentId, str.Value, fieldSymbol);
    //             stubIndexImpl.Members.AddStub(documentId, containingType, fieldSymbol);
    //             stubIndexImpl.SyntaxIndex.AddStub(documentId, field, fieldSymbol);
    //             break;
    //         }
    //         case IndexKey.Integer i:
    //         {
    //             var fieldSymbol = new IndexedSymbol(
    //                 field,
    //                 i.Value,
    //                 new LuaLazyType(fieldType),
    //                 containingType
    //             );
    //             stubIndexImpl.ShortNameIndex.AddStub(documentId, $"[{i.Value}]", fieldSymbol);
    //             stubIndexImpl.Members.AddStub(documentId, containingType, fieldSymbol);
    //             stubIndexImpl.SyntaxIndex.AddStub(documentId, field, fieldSymbol);
    //             break;
    //         }
    //     }
    // }
    //
    // private void ClassIndex(DocumentId documentId,
    //     LuaDocTagClassSyntax luaDocTagClassSyntax)
    // {
    //     var stubIndexImpl = Compilation.StubIndexImpl;
    //     if (luaDocTagClassSyntax.Name is { } name)
    //     {
    //         var classType = CreateLuaType(documentId, name.RepresentText,
    //             () => new LuaClass(name.RepresentText));
    //         var classSymbol = new NamedSymbol(
    //             luaDocTagClassSyntax,
    //             name.RepresentText,
    //             classType,
    //             Compilation.Builtin.Global
    //         );
    //         stubIndexImpl.ShortNameIndex.AddStub(documentId, name.RepresentText, classSymbol);
    //         stubIndexImpl.Members.AddStub(documentId, stubIndexImpl.Compilation.Builtin.Global, classSymbol);
    //         stubIndexImpl.SyntaxIndex.AddStub(documentId, luaDocTagClassSyntax, classSymbol);
    //
    //         if (luaDocTagClassSyntax.Body is not { } body) return;
    //         var context = stubIndexImpl.Compilation.SearchContext;
    //         foreach (var field in body.FieldList)
    //         {
    //             var indexKey = IndexKey.FromDocTypedField(field, context);
    //             AddFieldSymbol(documentId, indexKey, field, field.Type, classType);
    //         }
    //     }
    // }
    //
    // private void EnumIndex(DocumentId documentId,
    //     LuaDocTagEnumSyntax luaDocTagEnumSyntax)
    // {
    //     var stubIndexImpl = Compilation.StubIndexImpl;
    //     if (luaDocTagEnumSyntax.Name is { } name)
    //     {
    //         var enumType = CreateLuaType(documentId, name.RepresentText,
    //             () => new LuaEnum(name.RepresentText, luaDocTagEnumSyntax.BaseType));
    //         var enumSymbol = new NamedSymbol(
    //             name,
    //             name.RepresentText,
    //             enumType,
    //             stubIndexImpl.Compilation.Builtin.Global
    //         );
    //         stubIndexImpl.ShortNameIndex.AddStub(documentId, name.RepresentText, enumSymbol);
    //         stubIndexImpl.Members.AddStub(documentId, stubIndexImpl.Compilation.Builtin.Global, enumSymbol);
    //         stubIndexImpl.SyntaxIndex.AddStub(documentId, luaDocTagEnumSyntax, enumSymbol);
    //
    //         foreach (var field in luaDocTagEnumSyntax.FieldList)
    //         {
    //             if (field is { Name: { } fieldName })
    //             {
    //                 var fieldSymbol = new EnumFieldSymbol(
    //                     fieldName,
    //                     fieldName.RepresentText,
    //                     enumType
    //                 );
    //
    //                 stubIndexImpl.ShortNameIndex.AddStub(documentId, fieldName.RepresentText, fieldSymbol);
    //                 stubIndexImpl.Members.AddStub(documentId, enumType, fieldSymbol);
    //             }
    //         }
    //     }
    // }
    //
    // private void InterfaceIndex(DocumentId documentId,
    //     LuaDocTagInterfaceSyntax luaDocTagInterfaceSyntax)
    // {
    //     var stubIndexImpl = Compilation.StubIndexImpl;
    //     if (luaDocTagInterfaceSyntax.Name is { } name)
    //     {
    //         var interfaceType = CreateLuaType(documentId, name.RepresentText,
    //             () => new LuaInterface(name.RepresentText));
    //         var interfaceSymbol = new NamedSymbol(
    //             luaDocTagInterfaceSyntax,
    //             name.RepresentText,
    //             interfaceType,
    //             Compilation.Builtin.Global);
    //
    //         stubIndexImpl.ShortNameIndex.AddStub(documentId, name.RepresentText, interfaceSymbol);
    //         stubIndexImpl.Members.AddStub(documentId, stubIndexImpl.Compilation.Builtin.Global, interfaceSymbol);
    //         stubIndexImpl.SyntaxIndex.AddStub(documentId, luaDocTagInterfaceSyntax, interfaceSymbol);
    //
    //         if (luaDocTagInterfaceSyntax.Body is not { } body) return;
    //         var context = stubIndexImpl.Compilation.SearchContext;
    //         foreach (var field in body.FieldList)
    //         {
    //             var indexKey = IndexKey.FromDocTypedField(field, context);
    //             AddFieldSymbol(documentId, indexKey, field, field.Type, interfaceType);
    //         }
    //     }
    // }
    //
    // private void AliasIndex(DocumentId documentId,
    //     LuaDocTagAliasSyntax luaDocTagAliasSyntax)
    // {
    //     var stubIndexImpl = Compilation.StubIndexImpl;
    //     if (luaDocTagAliasSyntax.Name is { } name)
    //     {
    //         var context = stubIndexImpl.Compilation.SearchContext;
    //         var baseTy = context.Infer(luaDocTagAliasSyntax.Type);
    //         var aliasType = CreateLuaType(documentId, name.RepresentText,
    //             () => new LuaAlias(name.RepresentText, baseTy));
    //         var aliasSymbol = new NamedSymbol(
    //             luaDocTagAliasSyntax,
    //             name.RepresentText,
    //             aliasType,
    //             stubIndexImpl.Compilation.Builtin.Global
    //         );
    //
    //         stubIndexImpl.ShortNameIndex.AddStub(documentId, name.RepresentText, aliasSymbol);
    //         stubIndexImpl.Members.AddStub(documentId, stubIndexImpl.Compilation.Builtin.Global, aliasSymbol);
    //         stubIndexImpl.SyntaxIndex.AddStub(documentId, luaDocTagAliasSyntax, aliasSymbol);
    //     }
    // }
    //
    // private void FieldIndex(DocumentId documentId,
    //     LuaDocTagFieldSyntax luaDocTagFieldSyntax)
    // {
    //     var stubIndexImpl = Compilation.StubIndexImpl;
    //     var context = Compilation.SearchContext;
    //     var indexKey = IndexKey.FromDocField(luaDocTagFieldSyntax, context);
    //     ILuaType? classType = null;
    //     // if (luaDocTagFieldSyntax.PrevOfType<LuaDocTagClassSyntax>() is { Name : { } className })
    //     // {
    //     //     classType = stubIndexImpl.NamedTypeIndex.Get<LuaClass>(className.RepresentText).FirstOrDefault();
    //     // }
    //     // else if (luaDocTagFieldSyntax.PrevOfType<LuaDocTagInterfaceSyntax>() is { Name : { } interfaceName })
    //     // {
    //     //     classType = stubIndexImpl.NamedTypeIndex.Get<LuaInterface>(interfaceName.RepresentText).FirstOrDefault();
    //     // }
    //
    //     if (classType is not null)
    //     {
    //         AddFieldSymbol(documentId, indexKey, luaDocTagFieldSyntax, luaDocTagFieldSyntax.Type,
    //             classType);
    //     }
    // }
    //
    // private void MethodIndex(DocumentId documentId,
    //     LuaFuncStatSyntax luaFuncStatSyntax)
    // {
    //     if (luaFuncStatSyntax is { IsLocal: true, FuncBody: { } funcBody, LocalName: { Name: { } name } localName })
    //     {
    //         var localSymbol = new LocalSymbol(localName, name.RepresentText,
    //             new LuaLazyType(funcBody));
    //         Compilation.StubIndexImpl.SyntaxIndex.AddStub(documentId, localName, localSymbol);
    //     }
    // }
    //
    // private void ForIndex(DocumentId documentId,
    //     LuaForStatSyntax luaForStatSyntax)
    // {
    //     var stubIndexImpl = Compilation.StubIndexImpl;
    //     if (luaForStatSyntax.IteratorName is { Name: { } name } param)
    //     {
    //         var docTagParamSyntax = luaForStatSyntax.Comments.FirstOrDefault()?
    //             .DocList.OfType<LuaDocTagParamSyntax>()
    //             .FirstOrDefault();
    //         if (docTagParamSyntax is { Name: { } tagParamName }
    //             && string.Equals(tagParamName.RepresentText, name.RepresentText, StringComparison.Ordinal))
    //         {
    //             var localSymbol = new LocalSymbol(param, name.RepresentText, new LuaLazyType(docTagParamSyntax.Type));
    //             stubIndexImpl.SyntaxIndex.AddStub(documentId, param, localSymbol);
    //         }
    //         else
    //         {
    //             var localSymbol =
    //                 new LocalSymbol(param, name.RepresentText, new LuaLazyType(luaForStatSyntax.InitExpr));
    //             stubIndexImpl.SyntaxIndex.AddStub(documentId, param, localSymbol);
    //         }
    //     }
    // }
    //
    // private void ForRangeIndex(DocumentId documentId,
    //     LuaForRangeStatSyntax luaForRangeStatSyntax)
    // {
    //     var stubIndexImpl = Compilation.StubIndexImpl;
    //     var docTagParamSyntaxDictionary = luaForRangeStatSyntax.Comments.FirstOrDefault()?
    //         .DocList.OfType<LuaDocTagParamSyntax>()
    //         .Where(it => it.Name is not null)
    //         .ToDictionary(it => it.Name!.RepresentText, it => it.Type);
    //
    //     var iteratorExprList = luaForRangeStatSyntax.ExprList;
    //     var iteratorGenerator = iteratorExprList.ToList();
    //     foreach (var (iteratorName, i) in luaForRangeStatSyntax.IteratorNames.Select((it, i) => (it, i)))
    //     {
    //         if (iteratorName is { Name: { } name } param)
    //         {
    //             if (docTagParamSyntaxDictionary is not null
    //                 && docTagParamSyntaxDictionary.TryGetValue(name.RepresentText, out var type))
    //             {
    //                 var localSymbol = new LocalSymbol(iteratorName, name.RepresentText, new LuaLazyType(type));
    //                 stubIndexImpl.SyntaxIndex.AddStub(documentId, param, localSymbol);
    //             }
    //             else
    //             {
    //                 var iteratorParamSymbol = new LocalSymbol(iteratorName, name.RepresentText,
    //                     new LuaLazyIterType(iteratorGenerator, i));
    //                 stubIndexImpl.SyntaxIndex.AddStub(documentId, param, iteratorParamSymbol);
    //             }
    //         }
    //     }
    // }
    //
    // private LuaDocTagSyntax? FindDefineTypeTag(LuaStatSyntax stat, StubIndexImpl stubIndexImpl)
    // {
    //     var docList = stat.Comments.FirstOrDefault()?.DocList;
    //     if (docList is null) return null;
    //     foreach (var docTagSyntax in docList)
    //     {
    //         switch (docTagSyntax)
    //         {
    //             case LuaDocTagClassSyntax luaDocClassSyntax:
    //             {
    //                 return luaDocClassSyntax;
    //             }
    //             case LuaDocTagEnumSyntax luaDocEnumSyntax:
    //             {
    //                 return luaDocEnumSyntax;
    //             }
    //             case LuaDocTagInterfaceSyntax luaDocInterfaceSyntax:
    //             {
    //                 return luaDocInterfaceSyntax;
    //             }
    //             case LuaDocTagTypeSyntax luaDocTagSyntax:
    //             {
    //                 return luaDocTagSyntax;
    //             }
    //         }
    //     }
    //
    //     return null;
    // }
    //
    // private void ModifyNameDeclarationSymbol(LuaDocTagSyntax docTagSyntax,
    //     List<(LuaSyntaxElement, LuaSymbol)> nameList)
    // {
    //     var firstLocalNameTuple = nameList.FirstOrDefault()!;
    //     switch (docTagSyntax)
    //     {
    //         case LuaDocTagClassSyntax classSyntax:
    //         {
    //             if (classSyntax.Name is { } name)
    //             {
    //                 if (TryFindLuaType<LuaClass>(name.RepresentText, out var luaClass) &&
    //                     luaClass is not null)
    //                 {
    //                     firstLocalNameTuple.Item2 = new TypeDeclarationSymbol(firstLocalNameTuple.Item1, luaClass);
    //                 }
    //             }
    //
    //             break;
    //         }
    //         case LuaDocTagEnumSyntax enumSyntax:
    //         {
    //             if (enumSyntax.Name is { } name)
    //             {
    //                 if (TryFindLuaType<LuaEnum>(name.RepresentText, out var luaEnum) &&
    //                     luaEnum is not null)
    //                 {
    //                     firstLocalNameTuple.Item2 = new TypeDeclarationSymbol(firstLocalNameTuple.Item1, luaEnum);
    //                 }
    //             }
    //
    //             break;
    //         }
    //         case LuaDocTagInterfaceSyntax interfaceSyntax:
    //         {
    //             if (interfaceSyntax.Name is { } name)
    //             {
    //                 if (TryFindLuaType<LuaInterface>(name.RepresentText, out var luaInterface) &&
    //                     luaInterface is not null)
    //                 {
    //                     firstLocalNameTuple.Item2 = new TypeDeclarationSymbol(firstLocalNameTuple.Item1, luaInterface);
    //                 }
    //             }
    //
    //             break;
    //         }
    //         case LuaDocTagTypeSyntax typeSyntax:
    //         {
    //             var idCount = 0;
    //             foreach (var docTypeSyntax in typeSyntax.TypeList)
    //             {
    //                 if (idCount < nameList.Count)
    //                 {
    //                     var localNameTuple = nameList[idCount];
    //                     if (localNameTuple.Item1 is LuaLocalNameSyntax localNameSyntax)
    //                     {
    //                         localNameTuple.Item2 = new LocalSymbol(localNameTuple.Item1,
    //                             localNameSyntax.Name!.RepresentText, new LuaLazyType(docTypeSyntax));
    //                     }
    //                 }
    //
    //                 idCount++;
    //             }
    //
    //             break;
    //         }
    //     }
    // }
    //
    //
    // private void LocalIndex( DocumentId documentId,
    //     LuaLocalStatSyntax luaLocalStatSyntax)
    // {
    //     var count = luaLocalStatSyntax.NameList.Count();
    //     var lastValidExprId = -1;
    //     LuaExprSyntax? lastValidExpr = null;
    //     for (var i = 0; i < count; i++)
    //     {
    //         var localName = luaLocalStatSyntax.NameList.ElementAt(i);
    //         var expr = luaLocalStatSyntax.ExpressionList.ElementAtOrDefault(i);
    //         if (expr is not null)
    //         {
    //             lastValidExpr = expr;
    //             lastValidExprId = i;
    //         }
    //
    //         if (localName is { Name: { } name })
    //         {
    //             var localSymbol = new LocalSymbol(localName, name.RepresentText,
    //                 new LuaLazyType(lastValidExpr, i - lastValidExprId));
    //             Compilation.StubIndexImpl.SyntaxIndex.AddStub(documentId, localName, localSymbol);
    //         }
    //     }
    // }
    //
    // private void AssignIndex(DocumentId documentId,
    //     LuaAssignStatSyntax luaAssignStatSyntax)
    // {
    //     var stubIndexImpl = Compilation.StubIndexImpl;
    //     var declarationTree = Compilation.GetDeclarationTree(documentId);
    //     var count = luaAssignStatSyntax.VarList.Count();
    //     var lastValidExprId = -1;
    //     LuaExprSyntax? lastValidExpr = null;
    //     for (var i = 0; i < count; i++)
    //     {
    //         var varName = luaAssignStatSyntax.VarList.ElementAt(i);
    //         var expr = luaAssignStatSyntax.ExprList.ElementAtOrDefault(i);
    //         if (expr is not null)
    //         {
    //             lastValidExpr = expr;
    //             lastValidExprId = i;
    //         }
    //
    //         if (varName is LuaNameExprSyntax nameExpr)
    //         {
    //             var declaration = declarationTree.FindDeclaration(nameExpr);
    //             // 重新赋值, 不计算为新的符号
    //             if (declaration is not null && !Equals(declaration.SyntaxElement, nameExpr))
    //             {
    //                 continue;
    //             }
    //
    //             if (declaration is { IsGlobal: true } && nameExpr is { Name: { } name })
    //             {
    //                 var globalSymbol = new NamedSymbol(nameExpr, name.RepresentText,
    //                     new LuaLazyType(lastValidExpr, i - lastValidExprId), Compilation.Builtin.Global);
    //                 stubIndexImpl.ShortNameIndex.AddStub(documentId, name.RepresentText, globalSymbol);
    //                 stubIndexImpl.Members.AddStub(documentId, Compilation.Builtin.Global, globalSymbol);
    //                 stubIndexImpl.SyntaxIndex.AddStub(documentId, nameExpr, globalSymbol);
    //             }
    //         }
    //         else if (varName is LuaIndexExprSyntax indexExpr)
    //         {
    //             var virtualSymbol = new VirtualSymbol(new LuaLazyType(indexExpr, i - lastValidExprId), null);
    //             stubIndexImpl.SyntaxIndex.AddStub(documentId, indexExpr, virtualSymbol);
    //         }
    //     }
    // }
    //
    // private void TableIndex(DocumentId documentId,
    //     LuaTableExprSyntax luaTableExprSyntax)
    // {
    //     var stubIndexImpl = Compilation.StubIndexImpl;
    //     var tableName = $"{documentId.Guid}:{luaTableExprSyntax.UniqueId}";
    //     var tableTy = CreateLuaType(documentId, tableName,
    //         () => new LuaClass(tableName));
    //     var context = stubIndexImpl.Compilation.SearchContext;
    //     stubIndexImpl.NamedTypeIndex.AddStub(documentId, tableName, tableTy);
    //
    //     foreach (var field in luaTableExprSyntax.FieldList)
    //     {
    //         var indexKey = IndexKey.FromTableFieldExpr(field, context);
    //         if (indexKey is not null)
    //         {
    //             AddFieldSymbol(documentId, indexKey, field, field.Value, tableTy);
    //         }
    //     }
    // }
    //
    // private bool TryFindLuaType<TLuaType>(string name, out TLuaType? type)
    //     where TLuaType : ILuaNamedType
    // {
    //     var buildInTy = Compilation.Builtin.FromName(name);
    //     if (buildInTy is TLuaType luaClass)
    //     {
    //         type = luaClass;
    //         return true;
    //     }
    //
    //     if (Compilation.StubIndexImpl.NamedTypeIndex.Get<TLuaType>(name).FirstOrDefault() is { } ty)
    //     {
    //         type = ty;
    //         return true;
    //     }
    //
    //     type = default(TLuaType);
    //     return false;
    // }
    //
    // private TLuaType CreateLuaType<TLuaType>(DocumentId documentId, string name,
    //     Func<TLuaType> factory)
    //     where TLuaType : ILuaNamedType
    // {
    //     if (TryFindLuaType<TLuaType>(name, out var ty))
    //     {
    //         return ty!;
    //     }
    //
    //     var newTy = factory();
    //     Compilation.StubIndexImpl.NamedTypeIndex.AddStub(documentId, name, newTy);
    //     return newTy;
    // }
    //

}
