using LuaLanguageServer.CodeAnalysis.Compilation.Analyzer.Stub;
using LuaLanguageServer.CodeAnalysis.Compilation.Type;
using LuaLanguageServer.CodeAnalysis.Syntax.Node.SyntaxNodes;
using LuaLanguageServer.CodeAnalysis.Workspace;

namespace LuaLanguageServer.CodeAnalysis.Compilation.Analyzer.Declaration;

public class DeclarationAnalyzer(LuaCompilation compilation) : LuaAnalyzer(compilation)
{
    public List<DelayAnalyzeNode> DelayAnalyzeNodes { get; } = new();

    public override void Analyze(DocumentId documentId)
    {
        if (Compilation.GetSyntaxTree(documentId) is { } syntaxTree)
        {
            var builder = new DeclarationBuilder(documentId, syntaxTree, this);
            Compilation.DeclarationTrees[documentId] = builder.Build();
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
                var declarationTree = Compilation.GetDeclarationTree(documentId);
                if (declarationTree is not null)
                {
                    var parentTyName = namedType.Name;
                    var declaration = new Declaration(indexName, declarationTree.GetPosition(keyElement), keyElement,
                        DeclarationFlag.ClassMember, delayAnalyzeNode.Scope, delayAnalyzeNode.Prev,
                        delayAnalyzeNode.LuaType);
                    delayAnalyzeNode.Scope?.Add(declaration);
                    Compilation.StubIndexImpl.Members.AddStub(documentId, parentTyName, declaration);
                }
            }
        }
    }

    public override void RemoveCache(DocumentId documentId)
    {
        Compilation.DeclarationTrees.Remove(documentId);
    }
}
