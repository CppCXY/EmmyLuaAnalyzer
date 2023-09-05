using LuaLanguageServer.CodeAnalysis.Kind;
using LuaLanguageServer.CodeAnalysis.Syntax.Green;
using LuaLanguageServer.CodeAnalysis.Syntax.Node.SyntaxNodes;
using LuaLanguageServer.CodeAnalysis.Syntax.Tree;

namespace LuaLanguageServer.CodeAnalysis.Syntax.Node;

class SyntaxFactory
{
    public static LuaSyntaxNodeOrToken CreateSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxNode? parent)
    {
        if (greenNode.IsSyntaxNode)
        {
            return new LuaSyntaxNodeOrToken(greenNode.SyntaxKind switch
            {
                LuaSyntaxKind.Source => new LuaSourceSyntax(greenNode, tree),
                LuaSyntaxKind.Block => new LuaBlockSyntax(greenNode, tree, parent),
                LuaSyntaxKind.EmptyStat => new LuaEmptyStatSyntax(greenNode, tree, parent),
                LuaSyntaxKind.LocalStat => new LuaLocalStatSyntax(greenNode, tree, parent),
                LuaSyntaxKind.LocalFuncStat => new LuaFunctionStatSyntax(greenNode, tree, parent),
                LuaSyntaxKind.IfStat => new LuaIfStatSyntax(greenNode, tree, parent),
                LuaSyntaxKind.IfClauseStat => new LuaIfClauseStatSyntax(greenNode, tree, parent),
                LuaSyntaxKind.WhileStat => new LuaWhileStatSyntax(greenNode, tree, parent),
                LuaSyntaxKind.DoStat => new LuaDoStatSyntax(greenNode, tree, parent),
                LuaSyntaxKind.ForStat => new LuaForStatSyntax(greenNode, tree, parent),
                LuaSyntaxKind.ForRangeStat => new LuaForRangeStatSyntax(greenNode, tree, parent),
                LuaSyntaxKind.RepeatStat => new LuaRepeatStatSyntax(greenNode, tree, parent),
                LuaSyntaxKind.FuncStat => new LuaFunctionStatSyntax(greenNode, tree, parent),
                LuaSyntaxKind.LabelStat => new LuaLabelStatSyntax(greenNode, tree, parent),
                LuaSyntaxKind.BreakStat => new LuaBreakStatSyntax(greenNode, tree, parent),
                LuaSyntaxKind.ReturnStat => new LuaReturnStatSyntax(greenNode, tree, parent),
                LuaSyntaxKind.GotoStat => new LuaGotoStatSyntax(greenNode, tree, parent),
                LuaSyntaxKind.ExprStat => new LuaCallStatSyntax(greenNode, tree, parent),
                LuaSyntaxKind.AssignStat => new LuaAssignmentStatSyntax(greenNode, tree, parent),
                LuaSyntaxKind.UnknownStat => new LuaUnknownStatSyntax(greenNode, tree, parent),
                LuaSyntaxKind.SuffixExpr => new LuaSuffixExprSyntax(greenNode, tree, parent),
                LuaSyntaxKind.ParenExpr => new LuaParenExprSyntax(greenNode, tree, parent),
                LuaSyntaxKind.LiteralExpr => new LuaLiteralExprSyntax(greenNode, tree, parent),
                LuaSyntaxKind.ClosureExpr => new LuaClosureExprSyntax(greenNode, tree, parent),
                LuaSyntaxKind.UnaryExpr => new LuaUnaryExprSyntax(greenNode, tree, parent),
                LuaSyntaxKind.BinaryExpr => new LuaBinaryExprSyntax(greenNode, tree, parent),
                LuaSyntaxKind.TableExpr => new LuaTableExprSyntax(greenNode, tree, parent),
                LuaSyntaxKind.CallExpr => new LuaCallExprSyntax(greenNode, tree, parent),
                LuaSyntaxKind.IndexExpr => new LuaIndexExprSyntax(greenNode, tree, parent),
                LuaSyntaxKind.NameExpr => new LuaNameSyntax(greenNode, tree, parent),
                LuaSyntaxKind.VarDef => new LuaVarDefSyntax(greenNode, tree, parent),
                LuaSyntaxKind.TableFieldAssign => new LuaTableFieldSyntax(greenNode, tree, parent),
                LuaSyntaxKind.TableFieldValue => new LuaTableFieldSyntax(greenNode, tree, parent),
                LuaSyntaxKind.Attribute => new LuaAttributeSyntax(greenNode, tree, parent),
                LuaSyntaxKind.ParamList => new LuaParamListSyntax(greenNode, tree, parent),
                LuaSyntaxKind.Comment => new LuaCommentSyntax(greenNode, tree, parent),
                LuaSyntaxKind.DocClass => new LuaDocClassSyntax(greenNode, tree, parent),
                LuaSyntaxKind.DocEnum => new LuaDocEnumSyntax(greenNode, tree, parent),
                LuaSyntaxKind.DocInterface => new LuaDocInterfaceSyntax(greenNode, tree, parent),
                LuaSyntaxKind.DocAlias => new LuaDocAliasSyntax(greenNode, tree, parent),
                LuaSyntaxKind.DocField => new LuaDocFieldSyntax(greenNode, tree, parent),
                LuaSyntaxKind.DocEnumField => new LuaDocEnumFieldSyntax(greenNode, tree, parent),
                LuaSyntaxKind.DocType => new LuaDocType(greenNode, tree, parent),
                LuaSyntaxKind.DocParam => new LuaDocParamSyntax(greenNode, tree, parent),
                LuaSyntaxKind.DocReturn => new LuaDocReturnSyntax(greenNode, tree, parent),
                LuaSyntaxKind.DocGeneric => expr,
                LuaSyntaxKind.DocSee => expr,
                LuaSyntaxKind.DocDeprecated => expr,
                LuaSyntaxKind.DocCast => expr,
                LuaSyntaxKind.DocOverload => expr,
                LuaSyntaxKind.DocAsync => expr,
                LuaSyntaxKind.DocVisibility => expr,
                LuaSyntaxKind.DocOther => expr,
                LuaSyntaxKind.DocDiagnostic => expr,
                LuaSyntaxKind.DocVersion => expr,
                LuaSyntaxKind.DocAs => expr,
                LuaSyntaxKind.DocNodiscard => expr,
                LuaSyntaxKind.DocOperator => expr,
                LuaSyntaxKind.TypeArray => expr,
                LuaSyntaxKind.TypeUnion => expr,
                LuaSyntaxKind.TypeFun => expr,
                LuaSyntaxKind.TypeGeneric => expr,
                LuaSyntaxKind.TypeTuple => expr,
                LuaSyntaxKind.TypeTable => expr,
                LuaSyntaxKind.TypeParen => expr,
                LuaSyntaxKind.TypeLiteral => expr,
                LuaSyntaxKind.TypeName => expr,
                LuaSyntaxKind.TypedParameter => expr,
                LuaSyntaxKind.TypedField => expr,
                LuaSyntaxKind.DocGenericDeclareList => expr,
                _ => throw new Exception("Unexpected SyntaxKind")
            });
        }
        else
        {
            return new LuaSyntaxNodeOrToken(new LuaSyntaxToken(greenNode, tree, parent));
        }
    }
}
