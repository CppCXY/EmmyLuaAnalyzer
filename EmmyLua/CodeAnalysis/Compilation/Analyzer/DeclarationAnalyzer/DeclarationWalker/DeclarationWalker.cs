using EmmyLua.CodeAnalysis.Compilation.Analyzer.ResolveAnalyzer;
using EmmyLua.CodeAnalysis.Compilation.Declaration;
using EmmyLua.CodeAnalysis.Compilation.Search;
using EmmyLua.CodeAnalysis.Compilation.Type;
using EmmyLua.CodeAnalysis.Document;
using EmmyLua.CodeAnalysis.Syntax.Kind;
using EmmyLua.CodeAnalysis.Syntax.Node;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;
using EmmyLua.CodeAnalysis.Syntax.Walker;

namespace EmmyLua.CodeAnalysis.Compilation.Analyzer.DeclarationAnalyzer.DeclarationWalker;

public partial class DeclarationWalker(DeclarationContext declarationContext, SearchContext searchContext) : ILuaElementWalker
{
    private LuaCompilation Compilation => searchContext.Compilation;

    private LuaDocumentId DocumentId => declarationContext.DocumentId;

    public void WalkIn(LuaSyntaxElement node)
    {
        if (IsScopeOwner(node))
        {
            declarationContext.PushScope(node);
        }

        switch (node)
        {
            case LuaLocalStatSyntax localStatSyntax:
            {
                AnalyzeLocalStatDeclaration(localStatSyntax);
                break;
            }
            case LuaForRangeStatSyntax forRangeStatSyntax:
            {
                AnalyzeForRangeStatDeclaration(forRangeStatSyntax);
                break;
            }
            case LuaForStatSyntax forStatSyntax:
            {
                AnalyzeForStatDeclaration(forStatSyntax);
                break;
            }
            case LuaFuncStatSyntax funcStatSyntax:
            {
                AnalyzeMethodDeclaration(funcStatSyntax);
                break;
            }
            case LuaClosureExprSyntax closureExprSyntax:
            {
                AnalyzeClosureExpr(closureExprSyntax);
                break;
            }
            case LuaAssignStatSyntax assignStatSyntax:
            {
                AnalyzeAssignStatDeclaration(assignStatSyntax);
                break;
            }
            case LuaDocTagClassSyntax tagClassSyntax:
            {
                AnalyzeClassTagDeclaration(tagClassSyntax);
                break;
            }
            case LuaDocTagAliasSyntax tagAliasSyntax:
            {
                AnalyzeAliasTagDeclaration(tagAliasSyntax);
                break;
            }
            case LuaDocTagEnumSyntax tagEnumSyntax:
            {
                AnalyzeEnumTagDeclaration(tagEnumSyntax);
                break;
            }
            case LuaDocTagInterfaceSyntax tagInterfaceSyntax:
            {
                AnalyzeInterfaceTagDeclaration(tagInterfaceSyntax);
                break;
            }
            case LuaTableExprSyntax tableSyntax:
            {
                AnalyzeTableExprDeclaration(tableSyntax);
                break;
            }
            case LuaDocTableTypeSyntax tableTypeSyntax:
            {
                AnalyzeLuaTableType(tableTypeSyntax);
                break;
            }
            case LuaSourceSyntax sourceSyntax:
            {
                AnalyzeSource(sourceSyntax);
                break;
            }
            case LuaNameExprSyntax nameExpr:
            {
                AnalyzeNameExpr(nameExpr);
                break;
            }
            case LuaIndexExprSyntax indexExpr:
            {
                IndexIndexExpr(indexExpr);
                break;
            }
            case LuaDocNameTypeSyntax docNameType:
            {
                IndexDocNameType(docNameType);
                break;
            }
            case LuaDocTagMetaSyntax:
            {
                Compilation.Diagnostics.AddMeta(DocumentId);
                break;
            }
            case LuaDocTagDiagnosticSyntax diagnosticSyntax:
            {
                AnalyzeDiagnostic(diagnosticSyntax);
                break;
            }
            case LuaDocTagModuleSyntax moduleSyntax:
            {
                AnalyzeModule(moduleSyntax);
                break;
            }
        }
    }

    public void WalkOut(LuaSyntaxElement node)
    {
        if (IsScopeOwner(node))
        {
            declarationContext.PopScope();
        }
    }

    private static bool IsScopeOwner(LuaSyntaxElement element)
        => element is LuaBlockSyntax or LuaRepeatStatSyntax or LuaForRangeStatSyntax or LuaForStatSyntax
            or LuaClosureExprSyntax;

    private void AnalyzeSource(LuaSourceSyntax sourceSyntax)
    {
        if (sourceSyntax.Block is { } block)
        {
            declarationContext.AddUnResolved(new UnResolvedSource(DocumentId, block, ResolveState.UnResolveReturn));
        }
    }
}
