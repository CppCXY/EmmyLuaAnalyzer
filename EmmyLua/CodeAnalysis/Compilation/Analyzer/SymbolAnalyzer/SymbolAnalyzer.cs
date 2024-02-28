using EmmyLua.CodeAnalysis.Compilation.Infer;
using EmmyLua.CodeAnalysis.Compilation.Symbol;
using EmmyLua.CodeAnalysis.Compilation.Type;
using EmmyLua.CodeAnalysis.Document;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace EmmyLua.CodeAnalysis.Compilation.Analyzer.SymbolAnalyzer;

// 符号分析会根据当前的符号表, 反复分析符号的类型, 直到不动点
public class SymbolAnalyzer(LuaCompilation compilation) : LuaAnalyzer(compilation)
{
    private SearchContext Context => Compilation.SearchContext;

    // public override void Analyze(DocumentId documentId)
    // {
    //     if (Compilation.GetSymbolTree(documentId) is { } tree)
    //     {
    //         foreach (var symbol in tree.Symbols)
    //         {
    //             switch (symbol)
    //             {
    //                 case LocalDeclaration localDeclaration:
    //                 {
    //                     AnalyzeLocalDeclaration(localDeclaration, documentId);
    //                     break;
    //                 }
    //                 case GlobalDeclaration globalDeclaration:
    //                 {
    //                     AnalyzeGlobalDeclaration(globalDeclaration, documentId);
    //                     break;
    //                 }
    //                 case TableFieldDeclaration tableFieldDeclaration:
    //                 {
    //                     AnalyzeTableFieldDeclaration(tableFieldDeclaration, documentId);
    //                     break;
    //                 }
    //                 case IndexDeclaration indexDeclaration:
    //                 {
    //                     AnalyzeIndexDeclaration(indexDeclaration, documentId);
    //                     break;
    //                 }
    //                 case MethodDeclaration methodDeclaration:
    //                 {
    //                     AnalyzeMethodDeclaration(methodDeclaration, documentId);
    //                     break;
    //                 }
    //             }
    //         }
    //     }
    // }
    //
    // private void AnalyzeLocalDeclaration(LocalDeclaration localDeclaration, DocumentId documentId)
    // {
    //     switch (localDeclaration)
    //     {
    //         case { DeclarationType: null, ExprRef: { } exprRef }:
    //         {
    //             BindExprType(localDeclaration, exprRef, documentId);
    //             break;
    //         }
    //         case
    //         {
    //             DeclarationType: ILuaNamedType namedType, ExprRef: { Expr: LuaTableExprSyntax tableExpr, RetId: 0 }
    //         }:
    //         {
    //             NamedTypeMerge(namedType, tableExpr, documentId);
    //             break;
    //         }
    //     }
    // }
    //
    // private void NamedTypeMerge(ILuaNamedType namedType, LuaTableExprSyntax tableExpr, DocumentId documentId)
    // {
    //     var name = namedType.Name;
    //     var members = Compilation.ProjectIndex.Members.Get(Compilation.SearchContext.GetUniqueId(tableExpr));
    //     foreach (var symbol in members)
    //     {
    //         Compilation.ProjectIndex.Members.AddStub(documentId, name, symbol);
    //     }
    // }
    //
    // private void AnalyzeGlobalDeclaration(GlobalDeclaration globalDeclaration, DocumentId documentId)
    // {
    //     switch (globalDeclaration)
    //     {
    //         case { DeclarationType: null, ExprRef: { } exprRef }:
    //         {
    //             BindExprType(globalDeclaration, exprRef, documentId);
    //             break;
    //         }
    //         case
    //         {
    //             DeclarationType: ILuaNamedType namedType, ExprRef: { Expr: LuaTableExprSyntax tableExpr, RetId: 0 }
    //         }:
    //         {
    //             NamedTypeMerge(namedType, tableExpr, documentId);
    //             break;
    //         }
    //     }
    // }
    //
    // private void AnalyzeTableFieldDeclaration(TableFieldDeclaration tableFieldDeclaration, DocumentId documentId)
    // {
    //     if (tableFieldDeclaration.TableField.Value is not null)
    //     {
    //         BindExprType(tableFieldDeclaration, new LuaExprRef(tableFieldDeclaration.TableField.Value), documentId);
    //     }
    // }
    //
    // private void AnalyzeIndexDeclaration(IndexDeclaration indexDeclaration, DocumentId documentId)
    // {
    //     var indexExpr = indexDeclaration.IndexExpr;
    //     AnalyzeIndexExpr(indexDeclaration, indexExpr, documentId);
    // }
    //
    // private void AnalyzeIndexExpr(Declaration declaration, LuaIndexExprSyntax indexExpr, DocumentId documentId)
    // {
    //     if (indexExpr is { PrefixExpr: { } prefixExpr })
    //     {
    //         var prefixDeclaration =
    //             Compilation.GetSymbolTree(documentId)?.FindDeclaration(prefixExpr, Context);
    //         if (prefixDeclaration is { DeclarationType: ILuaNamedType namedType })
    //         {
    //             Compilation.ProjectIndex.Members.AddStub(documentId, namedType.Name, declaration);
    //         }
    //     }
    // }
    //
    // private void AnalyzeMethodDeclaration(MethodDeclaration methodDeclaration, DocumentId documentId)
    // {
    //     var luaMethod = methodDeclaration.MethodType;
    //     if (luaMethod.MainSignature.ReturnTypes is null)
    //     {
    //         var block = methodDeclaration.MethodDef?.FuncBody?.Block;
    //         if (block is not null)
    //         {
    //             var returns = Compilation.ProjectIndex.MainBlockReturns.Get(block).FirstOrDefault();
    //             if (returns is not null)
    //             {
    //                 ILuaType retTy = Compilation.Builtin.Unknown;
    //                 foreach (var retExpr in returns)
    //                 {
    //                     retTy = LuaUnion.UnionType(retTy, Compilation.SearchContext.Infer(retExpr));
    //                 }
    //
    //                 luaMethod.MainSignature.ReturnTypes = retTy;
    //             }
    //         }
    //     }
    //
    //     var indexExpr = methodDeclaration.IndexExprSyntax;
    //     if (indexExpr is not null)
    //     {
    //         AnalyzeIndexExpr(methodDeclaration, indexExpr, documentId);
    //     }
    // }

    //
    // private void BindExprType(Declaration declaration, LuaExprRef exprRef, DocumentId documentId)
    // {
    //     if (exprRef.Expr is LuaTableExprSyntax)
    //     {
    //         declaration.DeclarationType = exprRef.GetType(Context);
    //     }
    // }
}
