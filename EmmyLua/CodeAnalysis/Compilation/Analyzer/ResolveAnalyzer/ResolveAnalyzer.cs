using EmmyLua.CodeAnalysis.Compilation.Infer;
using EmmyLua.CodeAnalysis.Compilation.Type;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace EmmyLua.CodeAnalysis.Compilation.Analyzer.ResolveAnalyzer;

// 符号分析会根据当前的符号表, 反复分析符号的类型, 直到不动点
public class SymbolAnalyzer(LuaCompilation compilation) : LuaAnalyzer(compilation)
{
    private SearchContext SearchContext => Compilation.SearchContext;

    private Dictionary<LuaBlockSyntax, List<List<LuaExprSyntax>>> BlockReturns { get; } = new();

    public override void Analyze(AnalyzeContext analyzeContext)
    {
        bool changed;
        do
        {
            changed = false;
            foreach (var unResolved in analyzeContext.UnResolves)
            {
                if ((unResolved.ResolvedState & ResolveState.UnResolvedType) != 0)
                {
                    ResolveType(unResolved, ref changed);
                }
                else if ((unResolved.ResolvedState & ResolveState.UnResolvedIndex) != 0)
                {
                    ResolveIndex(unResolved, ref changed);
                }
                else if ((unResolved.ResolvedState & ResolveState.UnResolveReturn) != 0)
                {
                    ResolveReturn(unResolved, ref changed);
                }
            }
        } while (!changed);
    }

    private void ResolveType(UnResolved unResolved, ref bool changed)
    {
        if (unResolved is UnResolvedDeclaration unResolvedDeclaration)
        {
            var exprRef = unResolvedDeclaration.ExprRef;
            if (exprRef is not null)
            {
                var exprType = SearchContext.Infer(exprRef.Expr);
                if (!exprType.Equals(Builtin.Unknown))
                {
                    MergeType(unResolvedDeclaration, exprType, exprRef.RetId);
                    unResolved.ResolvedState &= ~ResolveState.UnResolvedType;
                    changed = true;
                }
            }
        }
    }

    private void ResolveIndex(UnResolved unResolved, ref bool changed)
    {
        if (unResolved is UnResolvedDeclaration unResolvedDeclaration)
        {
            var declaration = unResolvedDeclaration.Declaration;
            if (declaration.SyntaxElement is LuaIndexExprSyntax indexExpr)
            {
                var documentId = indexExpr.Tree.Document.Id;
                var ty = SearchContext.Infer(indexExpr);
                if (ty is LuaNamedType namedType)
                {
                    Compilation.ProjectIndex.AddMember(documentId, namedType.Name, declaration);
                    unResolved.ResolvedState &= ~ResolveState.UnResolvedIndex;
                    changed = true;
                }
            }
        }
    }

    private void ResolveReturn(UnResolved unResolved, ref bool changed)
    {
        if (unResolved is UnResolvedMethod unResolvedMethod)
        {
            var methodType = unResolvedMethod.MethodType;
            if (methodType.MainSignature.ReturnType.Equals(Builtin.Unknown))
            {
                unResolved.ResolvedState &= ~ResolveState.UnResolveReturn;
                changed = true;
                return;
            }

            var block = unResolvedMethod.Block;
            var retTypes = CollectBlockReturns(block);
            if (retTypes is LuaReturnType returnType)
            {
                methodType.MainSignature.ReturnType = returnType;
                unResolved.ResolvedState &= ~ResolveState.UnResolveReturn;
                changed = true;
            }
        }
        else if (unResolved is UnResolvedSource unResolvedSource)
        {
            var block = unResolvedSource.Block;
            var retTypes = CollectBlockReturns(block);
            if (retTypes is LuaReturnType returnType)
            {
                Compilation.ProjectIndex.AddExportType(unResolvedSource.DocumentId,
                    returnType.RetTypes.Count == 0
                        ? Builtin.Boolean
                        : returnType.RetTypes.First());

                unResolved.ResolvedState &= ~ResolveState.UnResolveReturn;
                changed = true;
            }
        }
    }

    private List<List<LuaExprSyntax>> CollectBlockReturns(LuaBlockSyntax mainBlock)
    {
        var cfg = SearchContext.Compilation.GetControlFlowGraph(mainBlock);
        if (cfg is null)
        {
            return new();
        }

        var prevNodes = cfg.GetPredecessors(cfg.ExitNode).ToList();
        throw new NotImplementedException();
    }

    private void MergeType(UnResolvedDeclaration unResolved, LuaType type, int retId)
    {
        if (type is LuaReturnType returnType)
        {
            var childTy = returnType.RetTypes.ElementAtOrDefault(retId);
            if (childTy is not null)
            {
                type = childTy;
            }
        }

        var declaration = unResolved.Declaration;

        if (declaration.DeclarationType is null)
        {
            declaration.DeclarationType = type;
        }
        else if (unResolved.IsTypeDeclaration && type is LuaTableLiteralType tableLiteralType)
        {
            var members = Compilation.ProjectIndex.GetMembers(tableLiteralType.TableId);
            var documentId = declaration.SyntaxElement?.Tree.Document.Id;
            if (documentId is { } id)
            {
                foreach (var member in members)
                {
                    Compilation.ProjectIndex.AddMember(id, member.Name, member);
                }
            }
        }
    }
}
