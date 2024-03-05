using EmmyLua.CodeAnalysis.Compilation.Infer;
using EmmyLua.CodeAnalysis.Compilation.Type;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace EmmyLua.CodeAnalysis.Compilation.Analyzer.ResolveAnalyzer;

// 符号分析会根据当前的符号表, 反复分析符号的类型, 直到不动点
public class SymbolAnalyzer(LuaCompilation compilation) : LuaAnalyzer(compilation)
{
    private SearchContext SearchContext => Compilation.SearchContext;

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
                    if (unResolved is UnResolvedDeclaration unResolvedDeclaration)
                    {
                        var exprRef = unResolvedDeclaration.ExprRef;
                        if (exprRef is not null)
                        {
                            var exprType = SearchContext.Infer(exprRef.Expr);
                            if (!exprType.Equals(Compilation.Builtin.Unknown))
                            {
                                MergeType(unResolvedDeclaration, exprType, exprRef.RetId);
                                unResolved.ResolvedState &= ~ResolveState.UnResolvedType;
                                changed = true;
                            }
                        }
                    }
                }
                else if ((unResolved.ResolvedState & ResolveState.UnResolvedIndex) != 0)
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
                else if ((unResolved.ResolvedState & ResolveState.UnResolveReturn) != 0)
                {

                }
            }
        } while (!changed);
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
            if (documentId is {} id)
            {
                foreach (var member in members)
                {
                    Compilation.ProjectIndex.AddMember(id, member.Name, member);
                }
            }
        }
    }
}
