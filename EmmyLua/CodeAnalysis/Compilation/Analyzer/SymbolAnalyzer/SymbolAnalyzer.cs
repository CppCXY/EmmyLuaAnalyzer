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
        do
        {
            changed = false;
            foreach (var unResolveDeclaration in analyzeContext.UnResolveDeclarations)
            {
                if ((unResolveDeclaration.ResolvedState & ResolveState.UnResolvedType) != 0)
                {
                    var exprRef = unResolveDeclaration.ExprRef;
                    if (exprRef is not null)
                    {
                        var exprType = SearchContext.Infer(exprRef.Expr);
                        if (!exprType.Equals(Compilation.Builtin.Unknown))
                        {
                            MergeType(unResolveDeclaration, exprType);
                            unResolveDeclaration.ResolvedState &= ~ResolveState.UnResolvedType;
                            changed = true;
                        }
                    }
                }
                else if ((unResolveDeclaration.ResolvedState & ResolveState.UnResolvedIndex) != 0)
                {
                    var declaration = unResolveDeclaration.Declaration;
                    if (declaration.SyntaxElement is LuaIndexExprSyntax indexExpr)
                    {
                        var documentId = indexExpr.Tree.Document.Id;
                        var ty = SearchContext.Infer(indexExpr);
                        if (ty is LuaNamedType namedType)
                        {
                            Compilation.ProjectIndex.Members.Add(documentId, namedType.Name, declaration);
                            unResolveDeclaration.ResolvedState &= ~ResolveState.UnResolvedIndex;
                            changed = true;
                        }
                    }
                }
                else if ((unResolveDeclaration.ResolvedState & ResolveState.UnResolveReturn) != 0)
                {
                    var declaration = unResolveDeclaration.Declaration;
                    var exprRef = unResolveDeclaration.ExprRef;
                    if (exprRef is not null)
                    {
                        var exprType = SearchContext.Infer(exprRef.Expr);
                        if (!exprType.Equals(Compilation.Builtin.Unknown))
                        {
                            MergeType(unResolveDeclaration, exprType);
                            unResolveDeclaration.ResolvedState &= ~ResolveState.UnResolveReturn;
                            changed = true;
                        }
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
        else if (unResolveDeclaration.IsTypeDeclaration && type is LuaTableLiteralType tableLiteralType)
        {
            var members = Compilation.ProjectIndex.Members.Get(tableLiteralType.TableId);
            var documentId = declaration.SyntaxElement?.Tree.Document.Id;
            if (documentId is {} id)
            {
                foreach (var member in members)
                {
                    Compilation.ProjectIndex.Members.Add(id, member.Name, member);
                }
            }
        }
    }
}
