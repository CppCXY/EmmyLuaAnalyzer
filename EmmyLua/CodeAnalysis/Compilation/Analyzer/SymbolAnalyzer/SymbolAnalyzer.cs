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
            localDeclaration.VarRefId = Context.GetUniqueId(localDeclaration.LocalName);
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
            globalDeclaration.VarRefId = Context.GetUniqueId(globalDeclaration.NameSyntax);
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
        tableFieldDeclaration.VarRefId = Context.GetUniqueId(tableFieldDeclaration.TableField);
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
            else if (prefixDeclaration?.VarRefId is { } varRefId && varRefId.Length != 0)
            {
                Compilation.Stub.Members.AddStub(documentId, varRefId, declaration);
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
}
