using System.Globalization;
using System.Text;
using EmmyLuaAnalyzer.CodeAnalysis.Compile.Source;
using EmmyLuaAnalyzer.CodeAnalysis.Kind;
using EmmyLuaAnalyzer.CodeAnalysis.Syntax.Diagnostic;
using EmmyLuaAnalyzer.CodeAnalysis.Syntax.Green;
using EmmyLuaAnalyzer.CodeAnalysis.Syntax.Node.SyntaxNodes;
using EmmyLuaAnalyzer.CodeAnalysis.Syntax.Tree;

namespace EmmyLuaAnalyzer.CodeAnalysis.Syntax.Node;

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
                LuaTokenKind.TkString => CalculateString(greenNode, tree, parent),
                LuaTokenKind.TkLongString => CalculateLongString(greenNode, tree, parent),
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

    private static LuaIntegerToken CalculateInt(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent)
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

        try
        {
            var value = hex
                ? Convert.ToInt64(text.ToString(), 16)
                : Convert.ToInt64(text.ToString(), 10);
            return new LuaIntegerToken(value, suffix, greenNode, tree, parent);
        }
        catch (OverflowException)
        {
            tree.PushDiagnostic(new Compile.Diagnostic.Diagnostic(DiagnosticSeverity.Error,
                $"The integer literal '{text}' is too large to be represented in type 'long'",
                new SourceRange(greenNode.Range.StartOffset, greenNode.Range.Length)));
            return new LuaIntegerToken(0, suffix, greenNode, tree, parent);
        }
    }

    private static LuaFloatToken CalculateFloat(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent)
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
    private static LuaComplexToken CalculateComplex(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent)
    {
        var text = tree.Source.Text.AsSpan(greenNode.Range.StartOffset, greenNode.Range.Length);
        // 裁剪掉complex的i
        text = text[..^1];
        return new LuaComplexToken(text.ToString(), greenNode, tree, parent);
    }

    private static LuaStringToken CalculateString(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent)
    {
        var text = tree.Source.Text.AsSpan(greenNode.Range.StartOffset, greenNode.Range.Length);
        if (text.Length < 2)
        {
            return new LuaStringToken(string.Empty, greenNode, tree, parent);
        }

        var sb = new StringBuilder(text.Length - 2);
        var delimiter = text[0];
        for (var i = 1; i < text.Length; i++)
        {
            switch (text[i])
            {
                case '\\':
                {
                    i++;
                    if (i >= text.Length)
                    {
                        tree.PushDiagnostic(new Compile.Diagnostic.Diagnostic(DiagnosticSeverity.Error,
                            "Unexpected end of string", new SourceRange(greenNode.Range.StartOffset + i - 1, 1)));
                        break;
                    }

                    switch (text[i])
                    {
                        case 'a':
                        {
                            sb.Append('\a');
                            break;
                        }
                        case 'b':
                        {
                            sb.Append('\b');
                            break;
                        }
                        case 'f':
                        {
                            sb.Append('\f');
                            break;
                        }
                        case 'n':
                        {
                            sb.Append('\n');
                            break;
                        }
                        case 'r':
                        {
                            sb.Append('\r');
                            break;
                        }
                        case 't':
                        {
                            sb.Append('\t');
                            break;
                        }
                        case 'v':
                        {
                            sb.Append('\v');
                            break;
                        }
                        case 'x':
                        {
                            if (i + 2 >= text.Length)
                            {
                                tree.PushDiagnostic(new Compile.Diagnostic.Diagnostic(DiagnosticSeverity.Error,
                                    "Unexpected end of string", new SourceRange(greenNode.Range.StartOffset + i, 1)));
                                break;
                            }

                            var hex = text[(i + 1)..(i + 3)];
                            // 检查hex合法性
                            if (!char.IsAsciiHexDigit(hex[0]) || !char.IsAsciiHexDigit(hex[1]))
                            {
                                tree.PushDiagnostic(new Compile.Diagnostic.Diagnostic(DiagnosticSeverity.Error,
                                    $"Invalid hex escape sequence '\\x{hex}'",
                                    new SourceRange(greenNode.Range.StartOffset + i, 2)));
                                break;
                            }

                            i += 2;
                            sb.Append((char)Convert.ToUInt16(hex.ToString(), 16));
                            break;
                        }
                        case 'u':
                        {
                            // 解析 \u{xxxx} 形式的unicode字符
                            if (i + 2 >= text.Length)
                            {
                                tree.PushDiagnostic(new Compile.Diagnostic.Diagnostic(DiagnosticSeverity.Error,
                                    "Unexpected end of string",
                                    new SourceRange(greenNode.Range.StartOffset + i - 1, 1)));
                                break;
                            }

                            var j = 1;
                            if (text[i + j] != '{')
                            {
                                tree.PushDiagnostic(new Compile.Diagnostic.Diagnostic(DiagnosticSeverity.Error,
                                    $"Missing unicode escape sequence start '{{', current '{text[i + j]}'",
                                    new SourceRange(greenNode.Range.StartOffset + i + j, 1)));
                                break;
                            }

                            j++;
                            while (i + j < text.Length && char.IsAsciiHexDigit(text[i + j]))
                            {
                                j++;
                            }

                            if (i + j >= text.Length)
                            {
                                tree.PushDiagnostic(new Compile.Diagnostic.Diagnostic(DiagnosticSeverity.Error,
                                    "Unexpected end of string",
                                    new SourceRange(greenNode.Range.StartOffset + i + j - 1, 1)));
                                break;
                            }

                            if (text[i + j] != '}')
                            {
                                tree.PushDiagnostic(new Compile.Diagnostic.Diagnostic(DiagnosticSeverity.Error,
                                    $"Missing unicode escape sequence end '}}', current '{text[i + j]}'",
                                    new SourceRange(greenNode.Range.StartOffset + i + j, 1)));
                                break;
                            }

                            var unicodeHex = text[(i + 2)..(i + j)];
                            i += j;
                            if (unicodeHex.Length > 8)
                            {
                                tree.PushDiagnostic(new Compile.Diagnostic.Diagnostic(DiagnosticSeverity.Error,
                                    $"Invalid unicode escape sequence '{unicodeHex}'",
                                    new SourceRange(greenNode.Range.StartOffset + i - j, unicodeHex.Length)));
                                break;
                            }

                            var codePoint = Convert.ToInt32(unicodeHex.ToString(), 16);
                            if (codePoint > 0x10FFFF)
                            {
                                tree.PushDiagnostic(new Compile.Diagnostic.Diagnostic(DiagnosticSeverity.Error,
                                    $"Invalid unicode escape sequence '{unicodeHex}', the code point is too large",
                                    new SourceRange(greenNode.Range.StartOffset + i - j, unicodeHex.Length)));
                                break;
                            }

                            sb.Append(char.ConvertFromUtf32(codePoint));
                            break;
                        }
                        case '\r' or '\n':
                        {
                            // 跳过换行符
                            break;
                        }
                        case '\\' or '\"' or '\'':
                        {
                            sb.Append(text[i]);
                            break;
                        }
                        case 'z':
                        {
                            // 跳过空白符
                            do
                            {
                                i++;
                                if (i >= text.Length)
                                {
                                    break;
                                }
                            } while (char.IsWhiteSpace(text[i]));

                            break;
                        }
                        default:
                        {
                            tree.PushDiagnostic(new Compile.Diagnostic.Diagnostic(DiagnosticSeverity.Error,
                                $"Invalid escape sequence '\\{text[i]}'",
                                new SourceRange(greenNode.Range.StartOffset + i, 1)));
                            break;
                        }
                    }

                    break;
                }
                default:
                {
                    if (text[i] == delimiter)
                    {
                        break;
                    }

                    sb.Append(text[i]);
                    break;
                }
            }
        }

        return new LuaStringToken(sb.ToString(), greenNode, tree, parent);
    }

    // parse [[xxxx]]
    private static LuaStringToken CalculateLongString(GreenNode greenNode, LuaSyntaxTree tree,
        LuaSyntaxElement? parent)
    {
        var text = tree.Source.Text.AsSpan(greenNode.Range.StartOffset, greenNode.Range.Length);
        if (text.Length < 4)
        {
            return new LuaStringToken(string.Empty, greenNode, tree, parent);
        }

        var equalNum = 0;
        var i = 0;
        if (text[i] != '[')
        {
            tree.PushDiagnostic(new Compile.Diagnostic.Diagnostic(DiagnosticSeverity.Error,
                $"Invalid long string start, expected '[', current '{text[i]}'",
                new SourceRange(greenNode.Range.StartOffset, 1)));
            return new LuaStringToken(string.Empty, greenNode, tree, parent);
        }

        i++;
        while (i < text.Length && text[i] == '=')
        {
            equalNum++;
            i++;
        }

        if (i >= text.Length || text[i] != '[')
        {
            tree.PushDiagnostic(new Compile.Diagnostic.Diagnostic(DiagnosticSeverity.Error,
                $"Invalid long string start, expected '[', current '{text[i]}'",
                new SourceRange(greenNode.Range.StartOffset, 1)));
            return new LuaStringToken(string.Empty, greenNode, tree, parent);
        }

        i++;

        if (text.Length < i + equalNum + 2)
        {
            tree.PushDiagnostic(new Compile.Diagnostic.Diagnostic(DiagnosticSeverity.Error,
                $"Invalid long string end, expected '{new string('=', equalNum)}]', current '{text[^1]}'",
                new SourceRange(greenNode.Range.StartOffset + text.Length - 1, 1)));
            return new LuaStringToken(string.Empty, greenNode, tree, parent);
        }

        var content = text[i..(text.Length - equalNum - 2)];

        return new LuaStringToken(content.ToString(), greenNode, tree, parent);
    }
}
