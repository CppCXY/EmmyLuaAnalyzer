using EmmyLua.CodeAnalysis.Compilation.Infer;
using EmmyLua.CodeAnalysis.Compilation.Symbol;
using EmmyLua.CodeAnalysis.Compilation.Type;
using EmmyLua.CodeAnalysis.Document;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace EmmyLua.CodeAnalysis.Compilation.Analyzer.SymbolAnalyzer;

// 符号分析会根据当前的符号表, 反复分析符号的类型, 直到不动点
public class SymbolAnalyzer(LuaCompilation compilation) : LuaAnalyzer(compilation)
{
    private SearchContext SearchContext => Compilation.SearchContext;

    public override void Analyze(AnalyzeContext analyzeContext)
    {
        bool changed;
        var unResolveDeclarations = analyzeContext.UnResolveDeclarations
            .ToDictionary(it => it, it => false);
        do
        {
            changed = false;
            foreach (var (unResolveDeclaration, resolved) in unResolveDeclarations)
            {
                if (resolved) continue;
                var declaration = unResolveDeclaration.Declaration;
                var exprRef = unResolveDeclaration.ExprRef;
                if (exprRef is not null)
                {
                    var exprType = SearchContext.Infer(exprRef.Expr);
                    if (!exprType.Equals(Compilation.Builtin.Unknown))
                    {
                        MergeType(unResolveDeclaration, exprType);
                        unResolveDeclarations[unResolveDeclaration] = true;
                        changed = true;
                    }
                }
            }
        } while (!changed);
    }

    private void MergeType(UnResolveDeclaration unResolveDeclaration, LuaType type)
    {
        var declaration = unResolveDeclaration.Declaration;
        if (declaration.DeclarationType is null)
        {
            declaration.DeclarationType = type;
        }
        else if (unResolveDeclaration.IsTypeDeclaration)
        {
            declaration.DeclarationType = declaration.DeclarationType.Union(type);
        }
    }
}
