using EmmyLua.CodeAnalysis.Syntax.Kind;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;
using EmmyLua.CodeAnalysis.Syntax.Tree;

namespace EmmyLua.CodeAnalysis.Syntax.Node;

public static class SyntaxFactory
{
    public static LuaSyntaxElement CreateSyntax(int index, LuaSyntaxTree tree)
    {
        var rawKind = tree.GetRawKind(index);
        if (IsNode(rawKind))
        {
            return GetSyntaxKind(rawKind) switch
            {
                LuaSyntaxKind.Source => new LuaSourceSyntax(index, tree),
                LuaSyntaxKind.Block => new LuaBlockSyntax(index, tree),
                LuaSyntaxKind.EmptyStat => new LuaEmptyStatSyntax(index, tree),
                LuaSyntaxKind.LocalStat => new LuaLocalStatSyntax(index, tree),
                LuaSyntaxKind.LocalFuncStat => new LuaFuncStatSyntax(index, tree),
                LuaSyntaxKind.IfStat => new LuaIfStatSyntax(index, tree),
                LuaSyntaxKind.IfClauseStat => new LuaIfClauseStatSyntax(index, tree),
                LuaSyntaxKind.WhileStat => new LuaWhileStatSyntax(index, tree),
                LuaSyntaxKind.DoStat => new LuaDoStatSyntax(index, tree),
                LuaSyntaxKind.ForStat => new LuaForStatSyntax(index, tree),
                LuaSyntaxKind.ForRangeStat => new LuaForRangeStatSyntax(index, tree),
                LuaSyntaxKind.RepeatStat => new LuaRepeatStatSyntax(index, tree),
                LuaSyntaxKind.FuncStat => new LuaFuncStatSyntax(index, tree),
                LuaSyntaxKind.LabelStat => new LuaLabelStatSyntax(index, tree),
                LuaSyntaxKind.BreakStat => new LuaBreakStatSyntax(index, tree),
                LuaSyntaxKind.ReturnStat => new LuaReturnStatSyntax(index, tree),
                LuaSyntaxKind.GotoStat => new LuaGotoStatSyntax(index, tree),
                LuaSyntaxKind.ExprStat => new LuaCallStatSyntax(index, tree),
                LuaSyntaxKind.AssignStat => new LuaAssignStatSyntax(index, tree),
                LuaSyntaxKind.UnknownStat => new LuaUnknownStatSyntax(index, tree),
                LuaSyntaxKind.CallArgList => new LuaCallArgListSyntax(index, tree),
                LuaSyntaxKind.ParenExpr => new LuaParenExprSyntax(index, tree),
                LuaSyntaxKind.LiteralExpr => new LuaLiteralExprSyntax(index, tree),
                LuaSyntaxKind.ClosureExpr => new LuaClosureExprSyntax(index, tree),
                LuaSyntaxKind.UnaryExpr => new LuaUnaryExprSyntax(index, tree),
                LuaSyntaxKind.BinaryExpr => new LuaBinaryExprSyntax(index, tree),
                LuaSyntaxKind.TableExpr => new LuaTableExprSyntax(index, tree),
                LuaSyntaxKind.CallExpr => new LuaCallExprSyntax(index, tree),
                LuaSyntaxKind.IndexExpr => new LuaIndexExprSyntax(index, tree),
                LuaSyntaxKind.NameExpr => new LuaNameExprSyntax(index, tree),
                LuaSyntaxKind.TableFieldAssign => new LuaTableFieldSyntax(index, tree),
                LuaSyntaxKind.TableFieldValue => new LuaTableFieldSyntax(index, tree),
                LuaSyntaxKind.Attribute => new LuaAttributeSyntax(index, tree),
                LuaSyntaxKind.ParamList => new LuaParamListSyntax(index, tree),
                LuaSyntaxKind.Comment => new LuaCommentSyntax(index, tree),
                LuaSyntaxKind.DocClass => new LuaDocTagClassSyntax(index, tree),
                LuaSyntaxKind.DocEnum => new LuaDocTagEnumSyntax(index, tree),
                LuaSyntaxKind.DocInterface => new LuaDocTagInterfaceSyntax(index, tree),
                LuaSyntaxKind.DocAlias => new LuaDocTagAliasSyntax(index, tree),
                LuaSyntaxKind.DocField => new LuaDocTagFieldSyntax(index, tree),
                LuaSyntaxKind.DocEnumField => new LuaDocTagEnumFieldSyntax(index, tree),
                LuaSyntaxKind.DocType => new LuaDocTagTypeSyntax(index, tree),
                LuaSyntaxKind.DocParam => new LuaDocTagParamSyntax(index, tree),
                LuaSyntaxKind.DocReturn => new LuaDocTagReturnSyntax(index, tree),
                LuaSyntaxKind.DocGeneric => new LuaDocTagGenericSyntax(index, tree),
                LuaSyntaxKind.DocSee => new LuaDocTagSeeSyntax(index, tree),
                LuaSyntaxKind.DocDeprecated => new LuaDocTagDeprecatedSyntax(index, tree),
                LuaSyntaxKind.DocCast => new LuaDocTagCastSyntax(index, tree),
                LuaSyntaxKind.DocOverload => new LuaDocTagOverloadSyntax(index, tree),
                LuaSyntaxKind.DocAsync => new LuaDocTagAsyncSyntax(index, tree),
                LuaSyntaxKind.DocVisibility => new LuaDocTagVisibilitySyntax(index, tree),
                LuaSyntaxKind.DocOther => new LuaDocTagOtherSyntax(index, tree),
                LuaSyntaxKind.DocDiagnostic => new LuaDocTagDiagnosticSyntax(index, tree),
                LuaSyntaxKind.DocVersion => new LuaDocTagVersionSyntax(index, tree),
                LuaSyntaxKind.DocAs => new LuaDocTagAsSyntax(index, tree),
                LuaSyntaxKind.DocNodiscard => new LuaDocTagNodiscardSyntax(index, tree),
                LuaSyntaxKind.DocOperator => new LuaDocTagOperatorSyntax(index, tree),
                LuaSyntaxKind.DocMeta => new LuaDocTagMetaSyntax(index, tree),
                LuaSyntaxKind.DocMapping => new LuaDocTagMappingSyntax(index, tree),
                LuaSyntaxKind.TypeArray => new LuaDocArrayTypeSyntax(index, tree),
                LuaSyntaxKind.TypeUnion => new LuaDocUnionTypeSyntax(index, tree),
                LuaSyntaxKind.TypeFun => new LuaDocFuncTypeSyntax(index, tree),
                LuaSyntaxKind.TypeGeneric => new LuaDocGenericTypeSyntax(index, tree),
                LuaSyntaxKind.TypeTuple => new LuaDocTupleTypeSyntax(index, tree),
                LuaSyntaxKind.TypeTable => new LuaDocTableTypeSyntax(index, tree),
                LuaSyntaxKind.TypeParen => new LuaDocParenTypeSyntax(index, tree),
                LuaSyntaxKind.TypeLiteral => new LuaDocLiteralTypeSyntax(index, tree),
                LuaSyntaxKind.TypeName => new LuaDocNameTypeSyntax(index, tree),
                LuaSyntaxKind.TypedParameter => new LuaDocTypedParamSyntax(index, tree),
                LuaSyntaxKind.TypeVariadic => new LuaDocVariadicTypeSyntax(index, tree),
                LuaSyntaxKind.TypeExpand => new LuaDocExpandTypeSyntax(index, tree),
                LuaSyntaxKind.TypeAggregate => new LuaDocAggregateTypeSyntax(index, tree),
                LuaSyntaxKind.TypeTemplate => new LuaDocTemplateTypeSyntax(index, tree),
                LuaSyntaxKind.DocDetailField => new LuaDocFieldSyntax(index, tree),
                LuaSyntaxKind.LocalName => new LuaLocalNameSyntax(index, tree),
                LuaSyntaxKind.ParamName => new LuaParamDefSyntax(index, tree),
                LuaSyntaxKind.DocBody => new LuaDocBodySyntax(index, tree),
                LuaSyntaxKind.DocModule => new LuaDocTagModuleSyntax(index, tree),
                LuaSyntaxKind.GenericParameter => new LuaDocGenericParamSyntax(index, tree),
                LuaSyntaxKind.GenericDeclareList => new LuaDocGenericDeclareListSyntax(index, tree),
                LuaSyntaxKind.Description => new LuaDescriptionSyntax(index, tree),
                LuaSyntaxKind.DiagnosticNameList => new LuaDocDiagnosticNameListSyntax(index, tree),
                LuaSyntaxKind.DocAttribute => new LuaDocAttributeSyntax(index, tree),
                LuaSyntaxKind.Version => new LuaDocVersionSyntax(index, tree),
                _ => throw new ArgumentException("Unexpected SyntaxKind")
            };
        }

        return GetTokenKind(rawKind) switch
        {
            LuaTokenKind.TkString or LuaTokenKind.TkLongString => new LuaStringToken(index, tree),
            LuaTokenKind.TkInt => new LuaIntegerToken(index, tree),
            LuaTokenKind.TkFloat => new LuaFloatToken(index, tree),
            LuaTokenKind.TkComplex => new LuaComplexToken(index, tree),
            LuaTokenKind.TkTrue or LuaTokenKind.TkFalse => new LuaBoolToken(index, tree),
            LuaTokenKind.TkNil => new LuaNilToken(index, tree),
            LuaTokenKind.TkDots => new LuaDotsToken(index, tree),
            LuaTokenKind.TkName => new LuaNameToken(index, tree),
            LuaTokenKind.TkWhitespace => new LuaWhitespaceToken(index, tree),
            LuaTokenKind.TkVersionNumber => new LuaVersionNumberToken(index, tree),
            LuaTokenKind.TkTypeTemplate => new LuaTemplateTypeToken(index, tree),
            _ => new LuaSyntaxToken(index, tree)
        };
    }

    private static bool IsNode(int rawKind) => rawKind >> 16 == 1;

    private static LuaSyntaxKind GetSyntaxKind(int rawKind) => (LuaSyntaxKind)(rawKind & 0xFFFF);

    private static LuaTokenKind GetTokenKind(int rawKind) => (LuaTokenKind)(rawKind & 0xFFFF);
}
