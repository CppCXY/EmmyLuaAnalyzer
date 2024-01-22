using EmmyLua.CodeAnalysis.Compilation.Type;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;
using EmmyLua.CodeAnalysis.Compilation.Symbol;
using EmmyLua.CodeAnalysis.Document;

namespace EmmyLua.CodeAnalysis.Compilation.Analyzer.Declaration;

public class DeclarationAnalyzer(LuaCompilation compilation) : LuaAnalyzer(compilation)
{
    public List<DelayAnalyzeNode> DelayAnalyzeNodes { get; } = new();

    public override void Analyze(DocumentId documentId)
    {
        if (Compilation.GetSyntaxTree(documentId) is { } syntaxTree)
        {
            var builder = new DeclarationBuilder(documentId, syntaxTree, this);
            Compilation.SymbolTrees[documentId] = builder.Build();
        }

        DelayAnalyze();
    }

    private void DelayAnalyze()
    {
        foreach (var node in DelayAnalyzeNodes)
        {
            switch (node.Node)
            {
                case LuaIndexExprSyntax indexExprSyntax:
                {
                    IndexExprAnalyze(indexExprSyntax, node.DocumentId, node);
                    break;
                }
            }
        }

        DelayAnalyzeNodes.Clear();
    }

    private void IndexExprAnalyze(LuaIndexExprSyntax expr, DocumentId documentId, DelayAnalyzeNode delayAnalyzeNode)
    {
        if (expr is { Name: { } indexName, KeyElement: { } keyElement })
        {
            var prefixTy = Compilation.SearchContext.Infer(expr.PrefixExpr);
            if (prefixTy is ILuaNamedType namedType)
            {
                var declarationTree = Compilation.GetSymbolTree(documentId);
                if (declarationTree is not null)
                {
                    var parentTyName = namedType.Name;
                    var declaration = new Symbol.Symbol(indexName, declarationTree.GetPosition(keyElement), keyElement,
                        SymbolFlag.ClassMember, delayAnalyzeNode.Scope, delayAnalyzeNode.Prev,
                        delayAnalyzeNode.LuaType);
                    delayAnalyzeNode.Scope?.Add(declaration);
                    Compilation.StubIndexImpl.Members.AddStub(documentId, parentTyName, declaration);
                }
            }

            if (delayAnalyzeNode.LuaType is LuaMethod method && expr.IsColonIndex)
            {
                var declarationTree = Compilation.GetSymbolTree(documentId);
                if (declarationTree is not null)
                {
                    method.SelfType = prefixTy;
                }
            }
        }
    }

    public override void RemoveCache(DocumentId documentId)
    {
        Compilation.SymbolTrees.Remove(documentId);
    }
}
