using System.Globalization;
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
                LuaSyntaxKind.DocClass => new LuaDocTagClassSyntax(greenNode, tree, parent),
                LuaSyntaxKind.DocEnum => new LuaDocTagEnumSyntax(greenNode, tree, parent),
                LuaSyntaxKind.DocInterface => new LuaDocTagInterfaceSyntax(greenNode, tree, parent),
                LuaSyntaxKind.DocAlias => new LuaDocTagAliasSyntax(greenNode, tree, parent),
                LuaSyntaxKind.DocField => new LuaDocTagFieldSyntax(greenNode, tree, parent),
                LuaSyntaxKind.DocEnumField => new LuaDocTagEnumFieldSyntax(greenNode, tree, parent),
                LuaSyntaxKind.DocType => new LuaDocTagTypeSyntax(greenNode, tree, parent),
                LuaSyntaxKind.DocParam => new LuaDocTagParamSyntax(greenNode, tree, parent),
                LuaSyntaxKind.DocReturn => new LuaDocTagReturnSyntax(greenNode, tree, parent),
                LuaSyntaxKind.DocGeneric => new LuaDocTagGenericSyntax(greenNode, tree, parent),
                LuaSyntaxKind.DocSee => new LuaDocTagSeeSyntax(greenNode, tree, parent),
                LuaSyntaxKind.DocDeprecated => new LuaDocTagDeprecatedSyntax(greenNode, tree, parent),
                LuaSyntaxKind.DocCast => new LuaDocTagCastSyntax(greenNode, tree, parent),
                LuaSyntaxKind.DocOverload => new LuaDocTagOverloadSyntax(greenNode, tree, parent),
                LuaSyntaxKind.DocAsync => new LuaDocTagAsyncSyntax(greenNode, tree, parent),
                LuaSyntaxKind.DocVisibility => new LuaDocTagVisibilitySyntax(greenNode, tree, parent),
                LuaSyntaxKind.DocOther => new LuaDocTagOtherSyntax(greenNode, tree, parent),
                LuaSyntaxKind.DocDiagnostic => new LuaDocTagDiagnosticSyntax(greenNode, tree, parent),
                LuaSyntaxKind.DocVersion => new LuaDocTagVersionSyntax(greenNode, tree, parent),
                LuaSyntaxKind.DocAs => new LuaDocTagAsSyntax(greenNode, tree, parent),
                LuaSyntaxKind.DocNodiscard => new LuaDocTagNodiscardSyntax(greenNode, tree, parent),
                LuaSyntaxKind.DocOperator => new LuaDocTagOperatorSyntax(greenNode, tree, parent),
                LuaSyntaxKind.GenericDeclareList => new LuaDocTagGenericDeclareListSyntax(greenNode, tree, parent),
                LuaSyntaxKind.TypeArray => new LuaDocArrayTypeSyntax(greenNode, tree, parent),
                LuaSyntaxKind.TypeUnion => new LuaDocUnionTypeSyntax(greenNode, tree, parent),
                LuaSyntaxKind.TypeFun => new LuaDocFuncTypeSyntax(greenNode, tree, parent),
                LuaSyntaxKind.TypeGeneric => new LuaDocTagGenericSyntax(greenNode, tree, parent),
                LuaSyntaxKind.TypeTuple => new LuaDocTupleTypeSyntax(greenNode, tree, parent),
                LuaSyntaxKind.TypeTable => new LuaDocTableTypeSyntax(greenNode, tree, parent),
                LuaSyntaxKind.TypeParen => new LuaDocParenTypeSyntax(greenNode, tree, parent),
                LuaSyntaxKind.TypeLiteral => new LuaDocLiteralTypeSyntax(greenNode, tree, parent),
                LuaSyntaxKind.TypeName => new LuaDocNameTypeSyntax(greenNode, tree, parent),
                LuaSyntaxKind.TypedParameter => new LuaDocTagTypedParamSyntax(greenNode, tree, parent),
                LuaSyntaxKind.TypedField => new LuaDocTagTypedFieldSyntax(greenNode, tree, parent),
                LuaSyntaxKind.LocalName => new LuaLocalNameSyntax(greenNode, tree, parent),
                LuaSyntaxKind.ParamName => new LuaParamDefSyntax(greenNode, tree, parent),
                LuaSyntaxKind.TypeBody => new LuaDocTagBodySyntax(greenNode, tree, parent),
                LuaSyntaxKind.DocModule => new LuaDocTagModuleSyntax(greenNode, tree, parent),
                LuaSyntaxKind.GenericParameter => new LuaDocTagGenericParamSyntax(greenNode, tree, parent),
                LuaSyntaxKind.FuncBody => new LuaFuncBodySyntax(greenNode, tree, parent),
                _ => throw new ArgumentException("Unexpected SyntaxKind")
            };
        }
        else
        {
            return greenNode.TokenKind switch
            {
                LuaTokenKind.TkString or LuaTokenKind.TkLongString => new LuaStringToken(greenNode, tree, parent),
                LuaTokenKind.TkInt =>
                    CalculateInt(greenNode, tree, parent),
                LuaTokenKind.TkFloat =>
                    CalculateFloat(greenNode, tree, parent),
                LuaTokenKind.TkComplex =>
                    CalculateComplex(greenNode, tree, parent),
                LuaTokenKind.TkTrue or LuaTokenKind.TkFalse => new LuaBoolToken(greenNode, tree, parent),
                LuaTokenKind.TkNil => new LuaNilToken(greenNode, tree, parent),
                LuaTokenKind.TkDots => new LuaDotsToken(greenNode, tree, parent),
                LuaTokenKind.TkName => new LuaNameToken(greenNode, tree, parent),
                _ => new LuaSyntaxToken(greenNode, tree, parent)
            };
        }
    }

    private static LuaSyntaxElement CalculateInt(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent)
    {
        var hex = false;
        var text = tree.Source.Text.AsSpan(greenNode.Range.StartOffset, greenNode.Range.Length);
        if (text.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
        {
            hex = true;
        }

        var suffix = string.Empty;
        for (var i = 0; i < text.Length; i++)
        {
            if (text[i] is 'u' or 'l' or 'U' or 'L')
            {
                suffix = text[i..].ToString();
                text = text[..i];
                break;
            }
        }

        var value = hex
            ? Convert.ToUInt64(text.ToString(), 16)
            : Convert.ToUInt64(text.ToString(), 10);
        return new LuaIntegerToken(value, suffix, greenNode, tree, parent);
    }

    private static LuaSyntaxElement CalculateFloat(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent)
    {
        double value = 0;
        var text = tree.Source.Text.AsSpan(greenNode.Range.StartOffset, greenNode.Range.Length);
        // 支持16进制浮点数, C# 不能原生支持
        if (text.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
        {
            var hexFloatText = text[2..].ToString();
            var parts = hexFloatText.Split('.');
            var integerPart = long.Parse(parts[0], NumberStyles.AllowHexSpecifier);
            var fractionPart = long.Parse(parts[1], NumberStyles.AllowHexSpecifier);
            var fractionValue = fractionPart * Math.Pow(16, -parts[1].Length);
            value = integerPart + fractionValue;
            var exponentPosition = hexFloatText.IndexOf('p', StringComparison.CurrentCultureIgnoreCase);
            if (exponentPosition != 0)
            {
                var exponent = hexFloatText[(exponentPosition + 1)..];
                if (int.TryParse(exponent, out var exp))
                {
                    value *= Math.Pow(2, exp);
                }
            }
        }
        else
        {
            var exponent = string.Empty;
            for (var i = 0; i < text.Length; i++)
            {
                if (text[i] is 'e' or 'E')
                {
                    exponent = text[(i + 1)..].ToString();
                    text = text[..i];
                    break;
                }
            }

            if (double.TryParse(text.ToString(), out value))
            {
                if (exponent.Length != 0 && int.TryParse(exponent, out var exp))
                {
                    value *= Math.Pow(10, exp);
                }
            }
        }

        return new LuaFloatToken(value, greenNode, tree, parent);
    }

    // luajit 支持复数干嘛?
    private static LuaSyntaxElement CalculateComplex(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent)
    {
        var text = tree.Source.Text.AsSpan(greenNode.Range.StartOffset, greenNode.Range.Length);
        // 裁剪掉complex的i
        text = text[..^1];
        return new LuaComplexToken(text.ToString(), greenNode, tree, parent);
    }

    private static LuaSyntaxElement CalculateString(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent)
    {
        // var text = tree.Source.Text.AsSpan(greenNode.Range.StartOffset, greenNode.Range.Length);
        // // 裁剪掉complex的i
        // text = text[..^1];
        // return new LuaComplexToken(text.ToString(), greenNode, tree, parent);
        throw new NotImplementedException();
    }
}
