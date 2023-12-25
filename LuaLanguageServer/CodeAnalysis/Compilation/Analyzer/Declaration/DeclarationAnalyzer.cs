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
                case LuaTableFieldSyntax tableFieldSyntax:
                {
                    TableFieldAnalyze(tableFieldSyntax, node.DocumentId, node);
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
            if (expr.PrefixExpr is LuaNameExprSyntax nameExpr)
            {
                var declarationTree = Compilation.DeclarationTrees[documentId];
                var scope = declarationTree.FindScope(nameExpr);
                if (scope is null) return;
                var nameDeclaration = scope.FindNameExpr(nameExpr)?.FirstDeclaration;
                if (nameDeclaration is null) return;
                var ty = nameDeclaration.Type;
                var parentTyName = "";
                if (ty is ILuaNamedType namedType)
                {
                    parentTyName = namedType.Name;
                }
                else
                {
                    parentTyName = Compilation.SearchContext.GetUniqueId(nameDeclaration.SyntaxElement, documentId);
                }

                var declaration = new Declaration(indexName, declarationTree.GetPosition(keyElement), keyElement,
                    DeclarationFlag.ClassMember, delayAnalyzeNode.Scope, delayAnalyzeNode.Prev, delayAnalyzeNode.LuaType);
                delayAnalyzeNode.Scope?.Add(declaration);
                Compilation.StubIndexImpl.Members.AddStub(documentId, parentTyName, declaration);
            }
            else if (expr.PrefixExpr is LuaIndexExprSyntax indexExpr)
            {
                // TODO: AAA.BBB.CCC = 1 的形式暂时不支持, 因为需要真的做类型推断, 应该需要在bind分析之后
            }
        }
    }

    private void TableFieldAnalyze(LuaTableFieldSyntax field, DocumentId documentId, DelayAnalyzeNode delayAnalyzeNode)
    {
        if (field is { Name: { } fieldName, KeyElement: { } keyElement, ParentTable: { } table })
        {
            var declarationTree = Compilation.DeclarationTrees[documentId];
            var parentId = Compilation.SearchContext.GetUniqueId(table, documentId);

            var declaration = new Declaration(fieldName, declarationTree.GetPosition(field), field,
                DeclarationFlag.ClassMember, delayAnalyzeNode.Scope, delayAnalyzeNode.Prev, delayAnalyzeNode.LuaType);
            delayAnalyzeNode.Scope?.Add(declaration);
            Compilation.StubIndexImpl.Members.AddStub(documentId, parentId, declaration);
        }
    }

    public override void RemoveCache(DocumentId documentId)
    {
        Compilation.DeclarationTrees.Remove(documentId);
    }
}
