using System.Globalization;
using System.Text;
using EmmyLua.CodeAnalysis.Compile.Diagnostic;
using EmmyLua.CodeAnalysis.Document;
using EmmyLua.CodeAnalysis.Kind;
using EmmyLua.CodeAnalysis.Syntax.Green;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;
using EmmyLua.CodeAnalysis.Syntax.Tree;

namespace EmmyLua.CodeAnalysis.Syntax.Node;

public static class SyntaxFactory
{
    public static LuaSyntaxElement CreateSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent, int startOffset)
    {
        if (greenNode.IsNode)
        {
            return greenNode.SyntaxKind switch
            {
                LuaSyntaxKind.Source => new LuaSourceSyntax(greenNode, tree),
                LuaSyntaxKind.Block => new LuaBlockSyntax(greenNode, tree, parent, startOffset),
                LuaSyntaxKind.EmptyStat => new LuaEmptyStatSyntax(greenNode, tree, parent, startOffset),
                LuaSyntaxKind.LocalStat => new LuaLocalStatSyntax(greenNode, tree, parent, startOffset),
                LuaSyntaxKind.LocalFuncStat => new LuaFuncStatSyntax(greenNode, tree, parent, startOffset),
                LuaSyntaxKind.IfStat => new LuaIfStatSyntax(greenNode, tree, parent, startOffset),
                LuaSyntaxKind.IfClauseStat => new LuaIfClauseStatSyntax(greenNode, tree, parent, startOffset),
                LuaSyntaxKind.WhileStat => new LuaWhileStatSyntax(greenNode, tree, parent, startOffset),
                LuaSyntaxKind.DoStat => new LuaDoStatSyntax(greenNode, tree, parent, startOffset),
                LuaSyntaxKind.ForStat => new LuaForStatSyntax(greenNode, tree, parent, startOffset),
                LuaSyntaxKind.ForRangeStat => new LuaForRangeStatSyntax(greenNode, tree, parent, startOffset),
                LuaSyntaxKind.RepeatStat => new LuaRepeatStatSyntax(greenNode, tree, parent, startOffset),
                LuaSyntaxKind.FuncStat => new LuaFuncStatSyntax(greenNode, tree, parent, startOffset),
                LuaSyntaxKind.LabelStat => new LuaLabelStatSyntax(greenNode, tree, parent, startOffset),
                LuaSyntaxKind.BreakStat => new LuaBreakStatSyntax(greenNode, tree, parent, startOffset),
                LuaSyntaxKind.ReturnStat => new LuaReturnStatSyntax(greenNode, tree, parent, startOffset),
                LuaSyntaxKind.GotoStat => new LuaGotoStatSyntax(greenNode, tree, parent, startOffset),
                LuaSyntaxKind.ExprStat => new LuaCallStatSyntax(greenNode, tree, parent, startOffset),
                LuaSyntaxKind.AssignStat => new LuaAssignStatSyntax(greenNode, tree, parent, startOffset),
                LuaSyntaxKind.UnknownStat => new LuaUnknownStatSyntax(greenNode, tree, parent, startOffset),
                LuaSyntaxKind.CallArgList => new LuaCallArgListSyntax(greenNode, tree, parent, startOffset),
                LuaSyntaxKind.ParenExpr => new LuaParenExprSyntax(greenNode, tree, parent, startOffset),
                LuaSyntaxKind.LiteralExpr => new LuaLiteralExprSyntax(greenNode, tree, parent, startOffset),
                LuaSyntaxKind.ClosureExpr => new LuaClosureExprSyntax(greenNode, tree, parent, startOffset),
                LuaSyntaxKind.UnaryExpr => new LuaUnaryExprSyntax(greenNode, tree, parent, startOffset),
                LuaSyntaxKind.BinaryExpr => new LuaBinaryExprSyntax(greenNode, tree, parent, startOffset),
                LuaSyntaxKind.TableExpr => new LuaTableExprSyntax(greenNode, tree, parent, startOffset),
                LuaSyntaxKind.CallExpr => new LuaCallExprSyntax(greenNode, tree, parent, startOffset),
                LuaSyntaxKind.IndexExpr => new LuaIndexExprSyntax(greenNode, tree, parent, startOffset),
                LuaSyntaxKind.NameExpr => new LuaNameExprSyntax(greenNode, tree, parent, startOffset),
                LuaSyntaxKind.TableFieldAssign => new LuaTableFieldSyntax(greenNode, tree, parent, startOffset),
                LuaSyntaxKind.TableFieldValue => new LuaTableFieldSyntax(greenNode, tree, parent, startOffset),
                LuaSyntaxKind.Attribute => new LuaAttributeSyntax(greenNode, tree, parent, startOffset),
                LuaSyntaxKind.ParamList => new LuaParamListSyntax(greenNode, tree, parent, startOffset),
                LuaSyntaxKind.Comment => new LuaCommentSyntax(greenNode, tree, parent, startOffset),
                LuaSyntaxKind.DocClass => new LuaDocTagClassSyntax(greenNode, tree, parent, startOffset),
                LuaSyntaxKind.DocEnum => new LuaDocTagEnumSyntax(greenNode, tree, parent, startOffset),
                LuaSyntaxKind.DocInterface => new LuaDocTagInterfaceSyntax(greenNode, tree, parent, startOffset),
                LuaSyntaxKind.DocAlias => new LuaDocTagAliasSyntax(greenNode, tree, parent, startOffset),
                LuaSyntaxKind.DocField => new LuaDocTagFieldSyntax(greenNode, tree, parent, startOffset),
                LuaSyntaxKind.DocEnumField => new LuaDocTagEnumFieldSyntax(greenNode, tree, parent, startOffset),
                LuaSyntaxKind.DocType => new LuaDocTagTypeSyntax(greenNode, tree, parent, startOffset),
                LuaSyntaxKind.DocParam => new LuaDocTagParamSyntax(greenNode, tree, parent, startOffset),
                LuaSyntaxKind.DocReturn => new LuaDocTagReturnSyntax(greenNode, tree, parent, startOffset),
                LuaSyntaxKind.DocGeneric => new LuaDocTagGenericSyntax(greenNode, tree, parent, startOffset),
                LuaSyntaxKind.DocSee => new LuaDocTagSeeSyntax(greenNode, tree, parent, startOffset),
                LuaSyntaxKind.DocDeprecated => new LuaDocTagDeprecatedSyntax(greenNode, tree, parent, startOffset),
                LuaSyntaxKind.DocCast => new LuaDocTagCastSyntax(greenNode, tree, parent, startOffset),
                LuaSyntaxKind.DocOverload => new LuaDocTagOverloadSyntax(greenNode, tree, parent, startOffset),
                LuaSyntaxKind.DocAsync => new LuaDocTagAsyncSyntax(greenNode, tree, parent, startOffset),
                LuaSyntaxKind.DocVisibility => new LuaDocTagVisibilitySyntax(greenNode, tree, parent, startOffset),
                LuaSyntaxKind.DocOther => new LuaDocTagOtherSyntax(greenNode, tree, parent, startOffset),
                LuaSyntaxKind.DocDiagnostic => new LuaDocTagDiagnosticSyntax(greenNode, tree, parent, startOffset),
                LuaSyntaxKind.DocVersion => new LuaDocTagVersionSyntax(greenNode, tree, parent, startOffset),
                LuaSyntaxKind.DocAs => new LuaDocTagAsSyntax(greenNode, tree, parent, startOffset),
                LuaSyntaxKind.DocNodiscard => new LuaDocTagNodiscardSyntax(greenNode, tree, parent, startOffset),
                LuaSyntaxKind.DocOperator => new LuaDocTagOperatorSyntax(greenNode, tree, parent, startOffset),
                LuaSyntaxKind.TypeArray => new LuaDocArrayTypeSyntax(greenNode, tree, parent, startOffset),
                LuaSyntaxKind.TypeUnion => new LuaDocUnionTypeSyntax(greenNode, tree, parent, startOffset),
                LuaSyntaxKind.TypeFun => new LuaDocFuncTypeSyntax(greenNode, tree, parent, startOffset),
                LuaSyntaxKind.TypeGeneric => new LuaDocGenericTypeSyntax(greenNode, tree, parent, startOffset),
                LuaSyntaxKind.TypeTuple => new LuaDocTupleTypeSyntax(greenNode, tree, parent, startOffset),
                LuaSyntaxKind.TypeTable => new LuaDocTableTypeSyntax(greenNode, tree, parent, startOffset),
                LuaSyntaxKind.TypeParen => new LuaDocParenTypeSyntax(greenNode, tree, parent, startOffset),
                LuaSyntaxKind.TypeLiteral => new LuaDocLiteralTypeSyntax(greenNode, tree, parent, startOffset),
                LuaSyntaxKind.TypeName => new LuaDocNameTypeSyntax(greenNode, tree, parent, startOffset),
                LuaSyntaxKind.TypedParameter => new LuaDocTagTypedParamSyntax(greenNode, tree, parent, startOffset),
                LuaSyntaxKind.DocDetailField => new LuaDocFieldSyntax(greenNode, tree, parent, startOffset),
                LuaSyntaxKind.LocalName => new LuaLocalNameSyntax(greenNode, tree, parent, startOffset),
                LuaSyntaxKind.ParamName => new LuaParamDefSyntax(greenNode, tree, parent, startOffset),
                LuaSyntaxKind.DocBody => new LuaDocBodySyntax(greenNode, tree, parent, startOffset),
                LuaSyntaxKind.DocModule => new LuaDocTagModuleSyntax(greenNode, tree, parent, startOffset),
                LuaSyntaxKind.GenericParameter => new LuaDocGenericParamSyntax(greenNode, tree, parent, startOffset),
                LuaSyntaxKind.GenericDeclareList => new LuaDocGenericDeclareListSyntax(greenNode, tree, parent, startOffset),
                LuaSyntaxKind.Description => new LuaDescriptionSyntax(greenNode, tree, parent, startOffset),
                _ => throw new ArgumentException("Unexpected SyntaxKind")
            };
        }
        else
        {
            return greenNode.TokenKind switch
            {
                LuaTokenKind.TkString => CalculateString(greenNode, tree, parent, startOffset),
                LuaTokenKind.TkLongString => CalculateLongString(greenNode, tree, parent, startOffset),
                LuaTokenKind.TkInt =>
                    CalculateInt(greenNode, tree, parent, startOffset),
                LuaTokenKind.TkFloat =>
                    CalculateFloat(greenNode, tree, parent, startOffset),
                LuaTokenKind.TkComplex =>
                    CalculateComplex(greenNode, tree, parent, startOffset),
                LuaTokenKind.TkTrue or LuaTokenKind.TkFalse => new LuaBoolToken(greenNode, tree, parent, startOffset),
                LuaTokenKind.TkNil => new LuaNilToken(greenNode, tree, parent, startOffset),
                LuaTokenKind.TkDots => new LuaDotsToken(greenNode, tree, parent, startOffset),
                LuaTokenKind.TkName => new LuaNameToken(greenNode, tree, parent, startOffset),
                _ => new LuaSyntaxToken(greenNode, tree, parent, startOffset)
            };
        }
    }

    private static LuaIntegerToken CalculateInt(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent, int startOffset)
    {
        var hex = false;
        var text = tree.Document.Text.AsSpan(startOffset, greenNode.Length);
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
            return new LuaIntegerToken(value, suffix, greenNode, tree, parent, startOffset);
        }
        catch (OverflowException)
        {
            tree.PushDiagnostic(new Diagnostic(DiagnosticSeverity.Error,
                $"The integer literal '{text}' is too large to be represented in type 'long'",
                new SourceRange(startOffset, greenNode.Length)));
            return new LuaIntegerToken(0, suffix, greenNode, tree, parent, startOffset);
        }
        catch (Exception e)
        {
            tree.PushDiagnostic(new Diagnostic(DiagnosticSeverity.Error,
                $"The integer literal '{text}' is invalid, {e.Message}",
                new SourceRange(startOffset, greenNode.Length)));
            return new LuaIntegerToken(0, suffix, greenNode, tree, parent, startOffset);
        }
    }

    private static LuaFloatToken CalculateFloat(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent, int startOffset)
    {
        double value = 0;
        var text = tree.Document.Text.AsSpan(startOffset, greenNode.Length);
        // 支持16进制浮点数, C# 不能原生支持
        if (text.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
        {
            var hexFloatText = text[2..].ToString();
            var parts = hexFloatText.Split('.');
            long integerPart = 0;
            if (parts[0].Length != 0)
            {
                integerPart = long.Parse(parts[0], NumberStyles.AllowHexSpecifier);
            }

            long fractionPart = 0;
            if (parts[1].Length != 0)
            {
                fractionPart = long.Parse(parts[1], NumberStyles.AllowHexSpecifier);
            }

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

        return new LuaFloatToken(value, greenNode, tree, parent, startOffset);
    }

    // luajit 支持复数干嘛?
    private static LuaComplexToken CalculateComplex(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent, int startOffset)
    {
        var text = tree.Document.Text.AsSpan(startOffset, greenNode.Length);
        // 裁剪掉complex的i
        text = text[..^1];
        return new LuaComplexToken(text.ToString(), greenNode, tree, parent, startOffset);
    }

    private static LuaStringToken CalculateString(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent, int startOffset)
    {
        var text = tree.Document.Text.AsSpan(startOffset, greenNode.Length);
        if (text.Length < 2)
        {
            return new LuaStringToken(string.Empty, greenNode, tree, parent, startOffset);
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
                        tree.PushDiagnostic(new Diagnostic(DiagnosticSeverity.Error,
                            "Unexpected end of string", new SourceRange(startOffset + i - 1, 1)));
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
                                tree.PushDiagnostic(new Diagnostic(DiagnosticSeverity.Error,
                                    "Unexpected end of string", new SourceRange(startOffset + i, 1)));
                                break;
                            }

                            var hex = text[(i + 1)..(i + 3)];
                            // 检查hex合法性
                            if (!char.IsAsciiHexDigit(hex[0]) || !char.IsAsciiHexDigit(hex[1]))
                            {
                                tree.PushDiagnostic(new Diagnostic(DiagnosticSeverity.Error,
                                    $"Invalid hex escape sequence '\\x{hex}'",
                                    new SourceRange(startOffset + i, 2)));
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
                                tree.PushDiagnostic(new Diagnostic(DiagnosticSeverity.Error,
                                    "Unexpected end of string",
                                    new SourceRange(startOffset + i - 1, 1)));
                                break;
                            }

                            var j = 1;
                            if (text[i + j] != '{')
                            {
                                tree.PushDiagnostic(new Diagnostic(DiagnosticSeverity.Error,
                                    $"Missing unicode escape sequence start '{{', current '{text[i + j]}'",
                                    new SourceRange(startOffset + i + j, 1)));
                                break;
                            }

                            j++;
                            while (i + j < text.Length && char.IsAsciiHexDigit(text[i + j]))
                            {
                                j++;
                            }

                            if (i + j >= text.Length)
                            {
                                tree.PushDiagnostic(new Diagnostic(DiagnosticSeverity.Error,
                                    "Unexpected end of string",
                                    new SourceRange(startOffset + i + j - 1, 1)));
                                break;
                            }

                            if (text[i + j] != '}')
                            {
                                tree.PushDiagnostic(new Diagnostic(DiagnosticSeverity.Error,
                                    $"Missing unicode escape sequence end '}}', current '{text[i + j]}'",
                                    new SourceRange(startOffset + i + j, 1)));
                                break;
                            }

                            var unicodeHex = text[(i + 2)..(i + j)];
                            i += j;
                            if (unicodeHex.Length > 8)
                            {
                                tree.PushDiagnostic(new Diagnostic(DiagnosticSeverity.Error,
                                    $"Invalid unicode escape sequence '{unicodeHex}'",
                                    new SourceRange(startOffset + i - j, unicodeHex.Length)));
                                break;
                            }

                            try
                            {
                                if (unicodeHex.Length == 0)
                                {
                                    tree.PushDiagnostic(new Diagnostic(DiagnosticSeverity.Error,
                                        $"Invalid unicode escape sequence '{unicodeHex}'",
                                        new SourceRange(startOffset + i - j, unicodeHex.Length)));
                                    break;
                                }
                                var codePoint = Convert.ToInt32(unicodeHex.ToString(), 16);
                                if (codePoint > 0x10FFFF)
                                {
                                    tree.PushDiagnostic(new Diagnostic(DiagnosticSeverity.Error,
                                        $"Invalid unicode escape sequence '{unicodeHex}', the code point is too large",
                                        new SourceRange(startOffset + i - j, unicodeHex.Length)));
                                    break;
                                }

                                sb.Append(char.ConvertFromUtf32(codePoint));
                            }
                            catch (OverflowException)
                            {
                                tree.PushDiagnostic(new Diagnostic(DiagnosticSeverity.Error,
                                    $"Invalid unicode escape sequence '{unicodeHex}', the code point is too large",
                                    new SourceRange(startOffset + i - j, unicodeHex.Length)));
                            }

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
                                new SourceRange(startOffset + i, 1)));
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

        return new LuaStringToken(sb.ToString(), greenNode, tree, parent, startOffset);
    }

    // parse [[xxxx]]
    private static LuaStringToken CalculateLongString(GreenNode greenNode, LuaSyntaxTree tree,
        LuaSyntaxElement? parent, int startOffset)
    {
        var text = tree.Document.Text.AsSpan(startOffset, greenNode.Length);
        if (text.Length < 4)
        {
            return new LuaStringToken(string.Empty, greenNode, tree, parent, startOffset);
        }

        var equalNum = 0;
        var i = 0;
        if (text[i] != '[')
        {
            tree.PushDiagnostic(new Diagnostic(DiagnosticSeverity.Error,
                $"Invalid long string start, expected '[', current '{text[i]}'",
                new SourceRange(startOffset, 1)));
            return new LuaStringToken(string.Empty, greenNode, tree, parent, startOffset);
        }

        i++;
        while (i < text.Length && text[i] == '=')
        {
            equalNum++;
            i++;
        }

        if (i >= text.Length || text[i] != '[')
        {
            tree.PushDiagnostic(new Diagnostic(DiagnosticSeverity.Error,
                $"Invalid long string start, expected '[', current '{text[i]}'",
                new SourceRange(startOffset, 1)));
            return new LuaStringToken(string.Empty, greenNode, tree, parent, startOffset);
        }

        i++;

        if (text.Length < i + equalNum + 2)
        {
            tree.PushDiagnostic(new Diagnostic(DiagnosticSeverity.Error,
                $"Invalid long string end, expected '{new string('=', equalNum)}]', current '{text[^1]}'",
                new SourceRange(startOffset + text.Length - 1, 1)));
            return new LuaStringToken(string.Empty, greenNode, tree, parent, startOffset);
        }

        var content = text[i..(text.Length - equalNum - 2)];

        return new LuaStringToken(content.ToString(), greenNode, tree, parent, startOffset);
    }
}
