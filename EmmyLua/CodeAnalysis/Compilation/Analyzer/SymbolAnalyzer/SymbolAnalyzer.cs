using EmmyLua.CodeAnalysis.Compilation.Symbol;
using EmmyLua.CodeAnalysis.Compilation.Type;
using EmmyLua.CodeAnalysis.Document;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace EmmyLua.CodeAnalysis.Compilation.Analyzer.SymbolAnalyzer;

public class SymbolAnalyzer(LuaCompilation compilation) : LuaAnalyzer(compilation)
{
    public override void Analyze(DocumentId documentId)
    {
        if (Compilation.GetSymbolTree(documentId) is { } tree)
        {
            foreach (var symbol in tree.Symbols)
            {
                switch (symbol)
                {
                    case LocalDeclaration localDeclaration:
                    {
                        LocalDeclarationAnalyze(localDeclaration, documentId);
                        break;
                    }
                    case GlobalDeclaration globalDeclaration:
                    {
                        GlobalDeclarationAnalyze(globalDeclaration, documentId);
                        break;
                    }
                    case IndexDeclaration indexDeclaration:
                    {
                        IndexDeclarationAnalyze(indexDeclaration, documentId);
                        break;
                    }
                }
            }
        }
    }

    private void LocalDeclarationAnalyze(LocalDeclaration localDeclaration, DocumentId documentId)
    {
        if (localDeclaration.DeclarationType is null)
        {
            localDeclaration.DeclarationType = new LuaVarRef(localDeclaration.LocalName, localDeclaration.ExprRef);
        }
        else if (localDeclaration is
            { DeclarationType: ILuaNamedType namedType, ExprRef: { Expr: LuaTableExprSyntax tableExpr, RetId: 0 } })
        {
            NamedTypeMerge(namedType, tableExpr, documentId);
        }
    }

    private void NamedTypeMerge(ILuaNamedType namedType, LuaTableExprSyntax tableExpr, DocumentId documentId)
    {
        var name = namedType.Name;
        var members = Compilation.Stub.Members.Get(Compilation.SearchContext.GetUniqueId(tableExpr));
        foreach (var symbol in members)
        {
            Compilation.Stub.Members.AddStub(documentId, name, symbol);
        }
    }

    private void GlobalDeclarationAnalyze(GlobalDeclaration globalDeclaration, DocumentId documentId)
    {
        if (globalDeclaration.DeclarationType is null)
        {
            globalDeclaration.DeclarationType = new LuaVarRef(globalDeclaration.NameSyntax, globalDeclaration.ExprRef);
        }
        else if (globalDeclaration is
            { DeclarationType: ILuaNamedType namedType, ExprRef: { Expr: LuaTableExprSyntax tableExpr, RetId: 0 } })
        {
            NamedTypeMerge(namedType, tableExpr, documentId);
        }
    }

    //     var expr = node.IndexExpr;
    //     var documentId = node.DocumentId;
    //     if (node.IndexExpr is { Name: { } indexName, KeyElement: { } keyElement })
    //     {
    //         var prefixTy = Compilation.SearchContext.Infer(expr.PrefixExpr);
    //         if (prefixTy is ILuaNamedType namedType)
    //         {
    //             var symbolTree = Compilation.GetSymbolTree(documentId);
    //             if (symbolTree is not null)
    //             {
    //                 var parentTyName = namedType.Name;
    //                 var declaration;
    //                 Compilation.Stub.Members.AddStub(documentId, parentTyName, declaration);
    //             }
    //         }
    //
    //         if (node.LuaType is LuaMethod method && expr.IsColonIndex)
    //         {
    //             var declarationTree = Compilation.GetSymbolTree(documentId);
    //             if (declarationTree is not null)
    //             {
    //                 method.SelfType = prefixTy;
    //             }
    //         }
    //     }

    private void IndexDeclarationAnalyze(IndexDeclaration indexDeclaration, DocumentId documentId)
    {
        // if (indexDeclaration.DeclarationType is null)
        // {
        //     var prefixTy = Compilation.SearchContext.Infer(indexDeclaration.IndexExpr.PrefixExpr);
        //     if (prefixTy is ILuaNamedType namedType)
        //     {
        //         var symbolTree = Compilation.GetSymbolTree(documentId);
        //         if (symbolTree is not null)
        //         {
        //             var parentTyName = namedType.Name;
        //             var declaration = new GlobalDeclaration(indexDeclaration.Name, indexDeclaration.Position, indexDeclaration.IndexExpr.Name, null, null);
        //             Compilation.Stub.Members.AddStub(documentId, parentTyName, declaration);
        //         }
        //     }
        // }
        //
        // if (indexDeclaration.LuaType is LuaMethod method && indexDeclaration.IndexExpr.IsColonIndex)
        // {
        //     var declarationTree = Compilation.GetSymbolTree(documentId);
        //     if (declarationTree is not null)
        //     {
        //         method.SelfType = Compilation.SearchContext.Infer(indexDeclaration.IndexExpr.PrefixExpr);
        //     }
        // }
    }
}
