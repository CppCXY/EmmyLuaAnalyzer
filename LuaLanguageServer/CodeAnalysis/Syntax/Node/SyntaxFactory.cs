using LuaLanguageServer.CodeAnalysis.Kind;
using LuaLanguageServer.CodeAnalysis.Syntax.Green;
using LuaLanguageServer.CodeAnalysis.Syntax.Node.SyntaxNodes;
using LuaLanguageServer.CodeAnalysis.Syntax.Tree;

namespace LuaLanguageServer.CodeAnalysis.Syntax.Node;

public static class SyntaxFactory
{
    public static LuaSyntaxElement CreateSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent)
    {
        if (greenNode.IsSyntaxNode)
        {
            return greenNode.SyntaxKind switch
            {
                LuaSyntaxKind.Source => new LuaSourceSyntax(greenNode, tree),
                LuaSyntaxKind.Block => new LuaBlockSyntax(greenNode, tree, parent),
                LuaSyntaxKind.EmptyStat => new LuaEmptyStatSyntax(greenNode, tree, parent),
                LuaSyntaxKind.LocalStat => new LuaLocalStatSyntax(greenNode, tree, parent),
                LuaSyntaxKind.LocalFuncStat => new LuaFuncStatSyntax(greenNode, tree, parent),
                LuaSyntaxKind.IfStat => new LuaIfStatSyntax(greenNode, tree, parent),
                LuaSyntaxKind.IfClauseStat => new LuaIfClauseStatSyntax(greenNode, tree, parent),
                LuaSyntaxKind.WhileStat => new LuaWhileStatSyntax(greenNode, tree, parent),
                LuaSyntaxKind.DoStat => new LuaDoStatSyntax(greenNode, tree, parent),
                LuaSyntaxKind.ForStat => new LuaForStatSyntax(greenNode, tree, parent),
                LuaSyntaxKind.ForRangeStat => new LuaForRangeStatSyntax(greenNode, tree, parent),
                LuaSyntaxKind.RepeatStat => new LuaRepeatStatSyntax(greenNode, tree, parent),
                LuaSyntaxKind.FuncStat => new LuaFuncStatSyntax(greenNode, tree, parent),
                LuaSyntaxKind.MethodName => new LuaMethodNameSyntax(greenNode, tree, parent),
                LuaSyntaxKind.LabelStat => new LuaLabelStatSyntax(greenNode, tree, parent),
                LuaSyntaxKind.BreakStat => new LuaBreakStatSyntax(greenNode, tree, parent),
                LuaSyntaxKind.ReturnStat => new LuaReturnStatSyntax(greenNode, tree, parent),
                LuaSyntaxKind.GotoStat => new LuaGotoStatSyntax(greenNode, tree, parent),
                LuaSyntaxKind.ExprStat => new LuaCallStatSyntax(greenNode, tree, parent),
                LuaSyntaxKind.AssignStat => new LuaAssignStatSyntax(greenNode, tree, parent),
                LuaSyntaxKind.UnknownStat => new LuaUnknownStatSyntax(greenNode, tree, parent),
                LuaSyntaxKind.CallArgList => new LuaCallArgListSyntax(greenNode, tree, parent),
                LuaSyntaxKind.ParenExpr => new LuaParenExprSyntax(greenNode, tree, parent),
                LuaSyntaxKind.LiteralExpr => new LuaLiteralExprSyntax(greenNode, tree, parent),
                LuaSyntaxKind.ClosureExpr => new LuaClosureExprSyntax(greenNode, tree, parent),
                LuaSyntaxKind.UnaryExpr => new LuaUnaryExprSyntax(greenNode, tree, parent),
                LuaSyntaxKind.BinaryExpr => new LuaBinaryExprSyntax(greenNode, tree, parent),
                LuaSyntaxKind.TableExpr => new LuaTableExprSyntax(greenNode, tree, parent),
                LuaSyntaxKind.CallExpr => new LuaCallExprSyntax(greenNode, tree, parent),
                LuaSyntaxKind.IndexExpr => new LuaIndexExprSyntax(greenNode, tree, parent),
                LuaSyntaxKind.NameExpr => new LuaNameExprSyntax(greenNode, tree, parent),
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
                LuaSyntaxKind.DocType => new LuaDocTypeSyntax(greenNode, tree, parent),
                LuaSyntaxKind.DocParam => new LuaDocParamSyntax(greenNode, tree, parent),
                LuaSyntaxKind.DocReturn => new LuaDocReturnSyntax(greenNode, tree, parent),
                LuaSyntaxKind.DocGeneric => new LuaDocGenericSyntax(greenNode, tree, parent),
                LuaSyntaxKind.DocSee => new LuaDocSeeSyntax(greenNode, tree, parent),
                LuaSyntaxKind.DocDeprecated => new LuaDocDeprecatedSyntax(greenNode, tree, parent),
                LuaSyntaxKind.DocCast => new LuaDocCastSyntax(greenNode, tree, parent),
                LuaSyntaxKind.DocOverload => new LuaDocOverloadSyntax(greenNode, tree, parent),
                LuaSyntaxKind.DocAsync => new LuaDocAsyncSyntax(greenNode, tree, parent),
                LuaSyntaxKind.DocVisibility => new LuaDocVisibilitySyntax(greenNode, tree, parent),
                LuaSyntaxKind.DocOther => new LuaDocOtherSyntax(greenNode, tree, parent),
                LuaSyntaxKind.DocDiagnostic => new LuaDocDiagnosticSyntax(greenNode, tree, parent),
                LuaSyntaxKind.DocVersion => new LuaDocVersionSyntax(greenNode, tree, parent),
                LuaSyntaxKind.DocAs => new LuaDocAsSyntax(greenNode, tree, parent),
                LuaSyntaxKind.DocNodiscard => new LuaDocNodiscardSyntax(greenNode, tree, parent),
                LuaSyntaxKind.DocOperator => new LuaDocOperatorSyntax(greenNode, tree, parent),
                LuaSyntaxKind.DocGenericDeclareList => new LuaDocGenericDeclareListSyntax(greenNode, tree, parent),
                LuaSyntaxKind.TypeArray => new LuaDocArrayTypeSyntax(greenNode, tree, parent),
                LuaSyntaxKind.TypeUnion => new LuaDocUnionTypeSyntax(greenNode, tree, parent),
                LuaSyntaxKind.TypeFun => new LuaDocFuncTypeSyntax(greenNode, tree, parent),
                LuaSyntaxKind.TypeGeneric => new LuaDocGenericSyntax(greenNode, tree, parent),
                LuaSyntaxKind.TypeTuple => new LuaDocTupleTypeSyntax(greenNode, tree, parent),
                LuaSyntaxKind.TypeTable => new LuaDocTableTypeSyntax(greenNode, tree, parent),
                LuaSyntaxKind.TypeParen => new LuaDocParenTypeSyntax(greenNode, tree, parent),
                LuaSyntaxKind.TypeLiteral => new LuaDocLiteralTypeSyntax(greenNode, tree, parent),
                LuaSyntaxKind.TypeName => new LuaDocNameTypeSyntax(greenNode, tree, parent),
                LuaSyntaxKind.TypedParameter => new LuaDocTypedParamSyntax(greenNode, tree, parent),
                LuaSyntaxKind.TypedField => new LuaDocTypedFieldSyntax(greenNode, tree, parent),
                LuaSyntaxKind.LocalName => new LuaLocalNameSyntax(greenNode, tree, parent),
                LuaSyntaxKind.RequireExpr => new LuaRequireExprSyntax(greenNode, tree, parent),
                LuaSyntaxKind.ParamName => new LuaParamDefSyntax(greenNode, tree, parent),
                LuaSyntaxKind.TypeBody => new LuaDocBodySyntax(greenNode, tree, parent),
                _ => throw new Exception("Unexpected SyntaxKind")
            };
        }
        else
        {
            return greenNode.TokenKind switch
            {
                LuaTokenKind.TkString or LuaTokenKind.TkLongString => new LuaStringToken(greenNode, tree, parent),
                LuaTokenKind.TkInt or LuaTokenKind.TkNumber or LuaTokenKind.TkComplex => new LuaNumberToken(greenNode,
                    tree, parent),
                LuaTokenKind.TkTrue or LuaTokenKind.TkFalse => new LuaBoolToken(greenNode, tree, parent),
                LuaTokenKind.TkNil => new LuaNilToken(greenNode, tree, parent),
                LuaTokenKind.TkDots => new LuaDotsToken(greenNode, tree, parent),
                _ => new LuaSyntaxToken(greenNode, tree, parent)
            };
        }
    }
}
