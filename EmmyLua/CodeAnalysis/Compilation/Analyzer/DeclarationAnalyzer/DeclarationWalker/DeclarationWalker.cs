using EmmyLua.CodeAnalysis.Compilation.Analyzer.ResolveAnalyzer;
using EmmyLua.CodeAnalysis.Compile.Kind;
using EmmyLua.CodeAnalysis.Document;
using EmmyLua.CodeAnalysis.Syntax.Node;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace EmmyLua.CodeAnalysis.Compilation.Analyzer.DeclarationAnalyzer.DeclarationWalker;

public partial class DeclarationWalker(DeclarationBuilder builder, LuaCompilation compilation)
{
    private LuaCompilation Compilation => compilation;

    private LuaDocumentId DocumentId => builder.DocumentId;

    private LuaDocument Document => builder.Document;

    enum TraverseState
    {
        Enter,
        Leave
    }

    public void Walk(LuaSyntaxElement root)
    {
        var tree = root.Tree;
        var rootIt = root.Iter;
        var traverseStack = new Stack<(int, TraverseState)>();
        foreach (var it in rootIt.Children.Reverse())
        {
            traverseStack.Push((it.Index, TraverseState.Enter));
        }

        while (traverseStack.Count > 0)
        {
            var (itIndex, state) = traverseStack.Pop();
            if (state == TraverseState.Enter)
            {
                if (tree.IsNode(itIndex))
                {
                    var it = new SyntaxIterator(itIndex, tree);
                    if (IsScopeOwner(tree.GetSyntaxKind(itIndex)))
                    {
                        builder.PushScope(it);
                        traverseStack.Push((itIndex, TraverseState.Leave));
                    }

                    var element = tree.GetElement(itIndex);
                    if (element is not null)
                    {
                        WalkNode(element);
                    }

                    foreach (var child in it.Children.Reverse())
                    {
                        traverseStack.Push((child.Index, TraverseState.Enter));
                    }
                }
            }
            else
            {
                builder.PopScope();
            }
        }

        FinishAttachedAnalyze();
    }

    private void WalkNode(LuaSyntaxElement node)
    {
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
            case LuaDocTagMappingSyntax mappingSyntax:
            {
                AnalyzeSimpleTag(mappingSyntax);
                break;
            }
            case LuaDocTagNamespaceSyntax namespaceSyntax:
            {
                AnalyzeTagNamespace(namespaceSyntax);
                break;
            }
            case LuaDocTagUsingSyntax usingSyntax:
            {
                AnalyzeTagUsing(usingSyntax);
                break;
            }
            case LuaDocTagSourceSyntax sourceSyntax:
            {
                AnalyzeSimpleTag(sourceSyntax);
                break;
            }
        }
    }

    private static bool IsScopeOwner(LuaSyntaxKind kind) =>
        kind is LuaSyntaxKind.Block or LuaSyntaxKind.RepeatStat or LuaSyntaxKind.ForRangeStat or LuaSyntaxKind.ForStat
            or LuaSyntaxKind.ClosureExpr;

    private void AnalyzeSource(LuaSourceSyntax sourceSyntax)
    {
        if (sourceSyntax.Block is { } block)
        {
            builder.AddUnResolved(new UnResolvedSource(DocumentId, block, ResolveState.UnResolveReturn));
        }
    }
}
