using EmmyLua.CodeAnalysis.Compilation.Analyzer.Infer;
using EmmyLua.CodeAnalysis.Compile.Diagnostic;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;
using EmmyLua.CodeAnalysis.Workspace;
using EmmyLua.CodeAnalysis.Compilation.Type;
using EmmyLua.CodeAnalysis.Syntax.Node;
using EmmyLua.CodeAnalysis.Syntax.Tree;

namespace EmmyLua.CodeAnalysis.Compilation.Analyzer.Bind;

public class BindAnalyzer(LuaCompilation compilation) : LuaAnalyzer(compilation)
{
    private SearchContext Context => Compilation.SearchContext;
    private Dictionary<DocumentId, BindData> BindData { get; } = new();

    public override void Analyze(DocumentId documentId)
    {
        if (BindData.ContainsKey(documentId))
        {
            return;
        }

        var declarationTree = Compilation.GetDeclarationTree(documentId);
        if (declarationTree is null)
        {
            return;
        }

        var bindData = new BindData(documentId, declarationTree);
        BindData.Add(documentId, bindData);

        if (Compilation.GetSyntaxTree(documentId) is { } syntaxTree)
        {
            foreach (var node in syntaxTree.SyntaxRoot.Descendants)
            {
                switch (node)
                {
                    case LuaLocalStatSyntax luaLocalStat:
                    {
                        LocalBindAnalysis(luaLocalStat, bindData);
                        break;
                    }
                    case LuaAssignStatSyntax luaAssignStat:
                    {
                        AssignBindAnalysis(luaAssignStat, bindData);
                        break;
                    }
                }
            }
        }

        bindData.Step = BindAnalyzeStep.Finish;
    }

    private void LocalBindAnalysis(LuaLocalStatSyntax localStat, BindData bindData)
    {
        var tree = bindData.Tree;
        var nameList = localStat.NameList.ToList();
        var exprList = localStat.ExprList.ToList();
        var count = nameList.Count;
        for (var i = 0; i < count; i++)
        {
            var localName = nameList[i];
            var expr = exprList.ElementAtOrDefault(i);
            var exprType = Compilation.SearchContext.Infer(expr);
            var declaration = tree.FindDeclaration(localName);
            if (declaration is { Type: { } ty })
            {
                if (!exprType.SubTypeOf(ty, Context))
                {
                    localStat.Tree.PushDiagnostic(new Diagnostic(
                        DiagnosticSeverity.Warning,
                        DiagnosticCode.TypeNotMatch,
                        $"local {localName} type not match",
                        localName.Location
                    ));
                }
            }
            else
            {
                if (declaration != null) declaration.Type = exprType;
            }
        }
    }

    private void AssignBindAnalysis(LuaAssignStatSyntax assignStat, BindData bindData)
    {
        var tree = bindData.Tree;
        var varList = assignStat.VarList.ToList();
        var exprList = assignStat.ExprList.ToList();
        var count = varList.Count;
        for (var i = 0; i < count; i++)
        {
            var var = varList[i];
            var expr = exprList.ElementAtOrDefault(i);
            var exprType = Compilation.SearchContext.Infer(expr);
            var declaration = tree.FindDeclaration(var);
            if (declaration is { Type: { } ty })
            {
                if (!exprType.SubTypeOf(ty, Context))
                {
                    assignStat.Tree.PushDiagnostic(new Diagnostic(
                        DiagnosticSeverity.Warning,
                        DiagnosticCode.TypeNotMatch,
                        $"local {var} type not match",
                        var.Location
                    ));
                }
            }
            else
            {
                if (declaration != null) declaration.Type = exprType;
            }
        }
    }

    public override void RemoveCache(DocumentId documentId)
    {
        BindData.Remove(documentId);
    }
}
