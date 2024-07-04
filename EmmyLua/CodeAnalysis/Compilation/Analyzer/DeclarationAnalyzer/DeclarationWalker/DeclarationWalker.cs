using EmmyLua.CodeAnalysis.Compilation.Analyzer.ResolveAnalyzer;
using EmmyLua.CodeAnalysis.Compilation.Search;
using EmmyLua.CodeAnalysis.Document;
using EmmyLua.CodeAnalysis.Syntax.Node;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;
using EmmyLua.CodeAnalysis.Syntax.Walker;

namespace EmmyLua.CodeAnalysis.Compilation.Analyzer.DeclarationAnalyzer.DeclarationWalker;

public partial class DeclarationWalker(DeclarationContext declarationContext, SearchContext searchContext)
    : ILuaElementWalker
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
                AnalyzeLocalStat(localStatSyntax);
                break;
            }
            case LuaForRangeStatSyntax forRangeStatSyntax:
            {
                AnalyzeForRangeStat(forRangeStatSyntax);
                break;
            }
            case LuaForStatSyntax forStatSyntax:
            {
                AnalyzeForStat(forStatSyntax);
                break;
            }
            case LuaFuncStatSyntax funcStatSyntax:
            {
                AnalyzeMethod(funcStatSyntax);
                break;
            }
            case LuaClosureExprSyntax closureExprSyntax:
            {
                AnalyzeClosureExpr(closureExprSyntax);
                break;
            }
            case LuaAssignStatSyntax assignStatSyntax:
            {
                AnalyzeAssignStat(assignStatSyntax);
                break;
            }
            case LuaDocTagClassSyntax tagClassSyntax:
            {
                AnalyzeTagClass(tagClassSyntax);
                break;
            }
            case LuaDocTagAliasSyntax tagAliasSyntax:
            {
                AnalyzeTagAlias(tagAliasSyntax);
                break;
            }
            case LuaDocTagEnumSyntax tagEnumSyntax:
            {
                AnalyzeTagEnum(tagEnumSyntax);
                break;
            }
            case LuaDocTagInterfaceSyntax tagInterfaceSyntax:
            {
                AnalyzeTagInterface(tagInterfaceSyntax);
                break;
            }
            case LuaDocTagTypeSyntax typeSyntax:
            {
                AnalyzeTagType(typeSyntax);
                break;
            }
            case LuaTableExprSyntax tableSyntax:
            {
                AnalyzeTableExpr(tableSyntax);
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
                AnalyzeMeta();
                break;
            }
            case LuaDocTagDiagnosticSyntax diagnosticSyntax:
            {
                AnalyzeTagDiagnostic(diagnosticSyntax);
                break;
            }
            case LuaDocTagModuleSyntax moduleSyntax:
            {
                AnalyzeTagModule(moduleSyntax);
                break;
            }
            case LuaDocTagDeprecatedSyntax deprecatedSyntax:
            {
                AnalyzeSimpleTag(deprecatedSyntax);
                break;
            }
            case LuaDocTagNodiscardSyntax nodiscardSyntax:
            {
                AnalyzeSimpleTag(nodiscardSyntax);
                break;
            }
            case LuaDocTagAsyncSyntax asyncSyntax:
            {
                AnalyzeSimpleTag(asyncSyntax);
                break;
            }
            case LuaDocTagGenericSyntax genericSyntax:
            {
                AnalyzeSimpleTag(genericSyntax);
                break;
            }
            case LuaDocTagParamSyntax paramSyntax:
            {
                AnalyzeSimpleTag(paramSyntax);
                break;
            }
            case LuaDocTagReturnSyntax returnSyntax:
            {
                AnalyzeSimpleTag(returnSyntax);
                break;
            }
            case LuaDocTagSeeSyntax seeSyntax:
            {
                AnalyzeSimpleTag(seeSyntax);
                break;
            }
            case LuaDocTagAsSyntax asSyntax:
            {
                AnalyzeSimpleTag(asSyntax);
                break;
            }
            case LuaDocTagVisibilitySyntax visibilitySyntax:
            {
                AnalyzeSimpleTag(visibilitySyntax);
                break;
            }
            case LuaDocTagVersionSyntax versionSyntax:
            {
                AnalyzeSimpleTag(versionSyntax);
                break;
            }
            case LuaDocTagOverloadSyntax overloadSyntax:
            {
                AnalyzeSimpleTag(overloadSyntax);
                break;
            }
            case LuaDocTagMappingSyntax mappingSyntax:
            {
                AnalyzeSimpleTag(mappingSyntax);
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
