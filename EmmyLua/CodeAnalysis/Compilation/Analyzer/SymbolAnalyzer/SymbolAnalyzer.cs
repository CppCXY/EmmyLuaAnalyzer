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
                    case MethodDeclaration methodDeclaration:
                    {
                        MethodDeclarationAnalyze(methodDeclaration, documentId);
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

    private void GlobalDeclarationAnalyze(GlobalDeclaration globalDeclaration, DocumentId documentId)
    {
        if (globalDeclaration.DeclarationType is null)
        {
            globalDeclaration.DeclarationType = new LuaVarRef(globalDeclaration.NameSyntax, globalDeclaration.ExprRef);
        }
        else if (globalDeclaration is
                 {
                     DeclarationType: ILuaNamedType namedType, ExprRef: { Expr: LuaTableExprSyntax tableExpr, RetId: 0 }
                 })
        {
            NamedTypeMerge(namedType, tableExpr, documentId);
        }
    }

    private void IndexDeclarationAnalyze(IndexDeclaration indexDeclaration, DocumentId documentId)
    {
        var indexExpr = indexDeclaration.IndexExpr;
        if (indexExpr is { PrefixExpr: { } prefixExpr })
        {
            var ty = Compilation.SearchContext.Infer(prefixExpr);
            if (ty is ILuaNamedType namedType)
            {
                Compilation.Stub.Members.AddStub(documentId, namedType.Name, indexDeclaration);
            }
        }
    }

    private void MethodDeclarationAnalyze(MethodDeclaration methodDeclaration, DocumentId documentId)
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
        if (indexExpr is { PrefixExpr: { } prefixExpr })
        {
            var ty = Compilation.SearchContext.Infer(prefixExpr);
            if (ty is ILuaNamedType namedType)
            {
                Compilation.Stub.Members.AddStub(documentId, namedType.Name, methodDeclaration);
            }

            if (indexExpr.IsColonIndex)
            {
                luaMethod.SelfType = ty;
            }
        }
    }
}
