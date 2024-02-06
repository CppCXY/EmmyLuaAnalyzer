using EmmyLua.CodeAnalysis.Compilation.Infer;
using EmmyLua.CodeAnalysis.Compilation.Symbol;
using EmmyLua.CodeAnalysis.Compilation.Type;
using EmmyLua.CodeAnalysis.Document;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace EmmyLua.CodeAnalysis.Compilation.Analyzer.SymbolAnalyzer;

public class SymbolAnalyzer(LuaCompilation compilation) : LuaAnalyzer(compilation)
{
    private SearchContext Context => Compilation.SearchContext;

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
                        AnalyzeLocalDeclaration(localDeclaration, documentId);
                        break;
                    }
                    case GlobalDeclaration globalDeclaration:
                    {
                        AnalyzeGlobalDeclaration(globalDeclaration, documentId);
                        break;
                    }
                    case TableFieldDeclaration tableFieldDeclaration:
                    {
                        AnalyzeTableFieldDeclaration(tableFieldDeclaration, documentId);
                        break;
                    }
                    case IndexDeclaration indexDeclaration:
                    {
                        AnalyzeIndexDeclaration(indexDeclaration, documentId);
                        break;
                    }
                    case MethodDeclaration methodDeclaration:
                    {
                        AnalyzeMethodDeclaration(methodDeclaration, documentId);
                        break;
                    }
                }
            }
        }
    }

    private void AnalyzeLocalDeclaration(LocalDeclaration localDeclaration, DocumentId documentId)
    {
        if (localDeclaration.DeclarationType is null)
        {
            if (localDeclaration.ExprRef is not null)
            {
                BindExprType(localDeclaration, localDeclaration.ExprRef, documentId);
            }
        }
        else if (localDeclaration is
                 {
                     DeclarationType: ILuaNamedType namedType, ExprRef: { Expr: LuaTableExprSyntax tableExpr, RetId: 0 }
                 })
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

    private void AnalyzeGlobalDeclaration(GlobalDeclaration globalDeclaration, DocumentId documentId)
    {
        if (globalDeclaration.DeclarationType is null)
        {
            if (globalDeclaration.ExprRef is not null)
            {
                BindExprType(globalDeclaration, globalDeclaration.ExprRef, documentId);
            }
        }
        else if (globalDeclaration is
                 {
                     DeclarationType: ILuaNamedType namedType, ExprRef: { Expr: LuaTableExprSyntax tableExpr, RetId: 0 }
                 })
        {
            NamedTypeMerge(namedType, tableExpr, documentId);
        }
    }

    private void AnalyzeTableFieldDeclaration(TableFieldDeclaration tableFieldDeclaration, DocumentId documentId)
    {
        if (tableFieldDeclaration.TableField.Value is not null)
        {
            BindExprType(tableFieldDeclaration, new LuaExprRef(tableFieldDeclaration.TableField.Value), documentId);
        }
    }

    private void AnalyzeIndexDeclaration(IndexDeclaration indexDeclaration, DocumentId documentId)
    {
        var indexExpr = indexDeclaration.IndexExpr;
        AnalyzeIndexExpr(indexDeclaration, indexExpr, documentId);
    }

    private void AnalyzeIndexExpr(Declaration declaration, LuaIndexExprSyntax indexExpr, DocumentId documentId)
    {
        if (indexExpr is { PrefixExpr: { } prefixExpr })
        {
            var prefixDeclaration =
                Compilation.GetSymbolTree(documentId)?.FindDeclaration(prefixExpr, Context);
            if (prefixDeclaration is { DeclarationType: ILuaNamedType namedType })
            {
                Compilation.Stub.Members.AddStub(documentId, namedType.Name, declaration);
            }
        }
    }

    private void AnalyzeMethodDeclaration(MethodDeclaration methodDeclaration, DocumentId documentId)
    {
        var luaMethod = methodDeclaration.MethodType;
        if (luaMethod.MainSignature.ReturnTypes is null)
        {
            var block = methodDeclaration.MethodDef?.FuncBody?.Block;
            if (block is not null)
            {
                var returns = Compilation.Stub.MainBlockReturns.Get(block).FirstOrDefault();
                if (returns is not null)
                {
                    ILuaType retTy = Compilation.Builtin.Unknown;
                    foreach (var retExpr in returns)
                    {
                        retTy = LuaUnion.UnionType(retTy, Compilation.SearchContext.Infer(retExpr));
                    }

                    luaMethod.MainSignature.ReturnTypes = retTy;
                }
            }
        }

        var indexExpr = methodDeclaration.IndexExprSyntax;
        if (indexExpr is not null)
        {
            AnalyzeIndexExpr(methodDeclaration, indexExpr, documentId);
        }
    }


    private void BindExprType(Declaration declaration, LuaExprRef expr, DocumentId documentId)
    {
        // var ty = Compilation.SearchContext.Infer(expr.Expr);
        // declaration.DeclarationType = ty;
        // if (expr.RetId == 0)
        // {
        //     var name = expr.Expr switch
        //     {
        //         LuaNameExprSyntax nameExpr => nameExpr.Name.RepresentText,
        //         LuaIndexExprSyntax indexExpr => indexExpr.PrefixExpr switch
        //         {
        //             LuaNameExprSyntax nameExpr => nameExpr.Name.RepresentText,
        //             _ => null
        //         },
        //         _ => null
        //     };
        //     if (name is not null)
        //     {
        //         Compilation.Stub.GlobalDeclaration.AddStub(documentId, name, declaration);
        //     }
        // }
    }
}
