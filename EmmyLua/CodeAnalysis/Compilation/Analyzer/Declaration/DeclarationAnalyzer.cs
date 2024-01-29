using EmmyLua.CodeAnalysis.Compilation.Type;
using EmmyLua.CodeAnalysis.Compilation.Symbol;
using EmmyLua.CodeAnalysis.Document;

namespace EmmyLua.CodeAnalysis.Compilation.Analyzer.Declaration;

public class DeclarationAnalyzer(LuaCompilation compilation) : LuaAnalyzer(compilation)
{
    public override void Analyze(DocumentId documentId)
    {
        if (Compilation.GetSyntaxTree(documentId) is { } syntaxTree)
        {
            var builder = new DeclarationBuilder(documentId, syntaxTree, this);
            Compilation.SymbolTrees[documentId] = builder.Build();
        }
    }


    // foreach (var node in IndexNodes)
    // {
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
    //     break;
    // }
    //
    // IndexNodes.Clear();
}
