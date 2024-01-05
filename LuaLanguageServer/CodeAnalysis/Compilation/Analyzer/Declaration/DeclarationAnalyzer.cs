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
            switch (expr.PrefixExpr)
            {
                case LuaNameExprSyntax nameExpr:
                {
                    // TODO 优化
                    var declarationTree = Compilation.DeclarationTrees[documentId];
                    var scope = declarationTree.FindScope(nameExpr);
                    if (scope is null) return;
                    var nameDeclaration = scope.FindNameExpr(nameExpr)?.FirstDeclaration;
                    var parentTyName = "";
                    if (nameDeclaration is not null)
                    {
                        var ty = nameDeclaration.Type;
                        if (ty is ILuaNamedType namedType)
                        {
                            parentTyName = namedType.Name;
                        }
                        else
                        {
                            parentTyName =
                                Compilation.SearchContext.GetUniqueId(nameDeclaration.SyntaxElement, documentId);
                        }
                    }
                    else if (nameExpr.Name is { } name)
                    {
                        var globalDeclaration = Compilation.StubIndexImpl.GlobalDeclaration.Get<Declaration>(name.RepresentText).FirstOrDefault();
                        if (globalDeclaration is not null)
                        {
                            var ty = globalDeclaration.Type;
                            if (ty is ILuaNamedType namedType)
                            {
                                parentTyName = namedType.Name;
                            }
                            else
                            {
                                parentTyName =
                                    Compilation.SearchContext.GetUniqueId(globalDeclaration.SyntaxElement, documentId);
                            }
                        }
                    }

                    var declaration = new Declaration(indexName, declarationTree.GetPosition(keyElement), keyElement,
                        DeclarationFlag.ClassMember, delayAnalyzeNode.Scope, delayAnalyzeNode.Prev,
                        delayAnalyzeNode.LuaType);
                    delayAnalyzeNode.Scope?.Add(declaration);
                    Compilation.StubIndexImpl.Members.AddStub(documentId, parentTyName, declaration);
                    break;
                }
                case LuaIndexExprSyntax indexExpr:
                    // TODO: AAA.BBB.CCC = 1 的形式暂时不支持, 因为需要真的做类型推断, 应该需要在bind分析之后
                    break;
            }
        }
    }

    public override void RemoveCache(DocumentId documentId)
    {
        Compilation.DeclarationTrees.Remove(documentId);
    }
}
