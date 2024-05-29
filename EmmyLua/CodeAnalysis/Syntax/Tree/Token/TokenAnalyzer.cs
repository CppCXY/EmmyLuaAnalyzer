using System.Globalization;
using System.Text;
using EmmyLua.CodeAnalysis.Diagnostics;
using EmmyLua.CodeAnalysis.Document;
using EmmyLua.CodeAnalysis.Document.Version;
using EmmyLua.CodeAnalysis.Kind;

namespace EmmyLua.CodeAnalysis.Syntax.Tree.Token;

public static class TokenAnalyzer
{
    public static void Analyze(int redCount, LuaSyntaxTree tree)
    {
        for (int index = 0; index < redCount; index++)
        {
            switch (tree.GetTokenKind(index))
            {
                case LuaTokenKind.TkInt:
                {
                    CalculateInt(index, tree);
                    break;
                }
                case LuaTokenKind.TkFloat:
                {
                    CalculateFloat(index, tree);
                    break;
                }
                case LuaTokenKind.TkString:
                {
                    CalculateString(index, tree);
                    break;
                }
                case LuaTokenKind.TkLongString:
                {
                    CalculateLongString(index, tree);
                    break;
                }
                case LuaTokenKind.TkVersionNumber:
                {
                    CalculateVersionNumber(index, tree);
                    break;
                }
                case LuaTokenKind.TkTypeTemplate:
                {
                    CalculateTemplateType(index, tree);
                    break;
                }
            }
        }
    }

    private static void CalculateInt(int index, LuaSyntaxTree tree)
    {
        var hex = false;
        var sourceRange = tree.GetSourceRange(index);
        var text = tree.Document.Text.AsSpan(sourceRange.StartOffset, sourceRange.Length);
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
            tree.SetIntegerTokenValue(index, value, suffix);
        }
        catch (OverflowException)
        {
            tree.PushDiagnostic(new Diagnostic(DiagnosticSeverity.Error,
                DiagnosticCode.SyntaxError,
                $"The integer literal '{text}' is too large to be represented in type 'long'",
                sourceRange));
            tree.SetIntegerTokenValue(index, 0, suffix);
        }
        catch (Exception e)
        {
            tree.PushDiagnostic(new Diagnostic(DiagnosticSeverity.Error,
                DiagnosticCode.SyntaxError,
                $"The integer literal '{text}' is invalid, {e.Message}",
                sourceRange));
            tree.SetIntegerTokenValue(index, 0, suffix);
        }
    }

    private static void CalculateFloat(int index, LuaSyntaxTree tree)
    {
        double value = 0;
        var sourceRange = tree.GetSourceRange(index);
        var text = tree.Document.Text.AsSpan(sourceRange.StartOffset, sourceRange.Length);
        // 支持16进制浮点数, C# 不能原生支持
        if (text.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
        {
            var hexFloatText = text[2..].ToString();
            var exponentPosition = hexFloatText.IndexOf('p', StringComparison.CurrentCultureIgnoreCase);
            var floatPart = hexFloatText;
            if (exponentPosition != -1)
            {
                floatPart = hexFloatText[..exponentPosition];
            }

            long integerPart = 0;
            var fractionValue = 0.0;
            if (floatPart.IndexOf('.') != -1)
            {
                var parts = floatPart.Split('.');
                if (parts[0].Length != 0)
                {
                    integerPart = long.Parse(parts[0], NumberStyles.AllowHexSpecifier);
                }

                long fractionPart = 0;
                if (parts[1].Length != 0)
                {
                    fractionPart = long.Parse(parts[1], NumberStyles.AllowHexSpecifier);
                }

                fractionValue = fractionPart * Math.Pow(16, -parts[1].Length);
            }
            else
            {
                integerPart = long.Parse(floatPart, NumberStyles.AllowHexSpecifier);
            }

            value = integerPart + fractionValue;
            if (exponentPosition != -1)
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

        tree.SetNumberTokenValue(index, value);
    }

    private static void CalculateString(int index, LuaSyntaxTree tree)
    {
        var sourceRange = tree.GetSourceRange(index);
        var text = tree.Document.Text.AsSpan(sourceRange.StartOffset, sourceRange.Length);
        if (text.Length < 2)
        {
            tree.SetStringTokenValue(index, string.Empty);
            return;
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
                            DiagnosticCode.SyntaxError,
                            "Unexpected end of string",
                            new SourceRange(sourceRange.StartOffset + i - 1, 1)));
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
                                    DiagnosticCode.SyntaxError,
                                    "Unexpected end of string", new SourceRange(sourceRange.StartOffset + i, 1)));
                                break;
                            }

                            var hex = text[(i + 1)..(i + 3)];
                            // 检查hex合法性
                            if (!char.IsAsciiHexDigit(hex[0]) || !char.IsAsciiHexDigit(hex[1]))
                            {
                                tree.PushDiagnostic(new Diagnostic(DiagnosticSeverity.Error,
                                    DiagnosticCode.SyntaxError,
                                    $"Invalid hex escape sequence '\\x{hex}'",
                                    new SourceRange(sourceRange.StartOffset + i, 2)));
                                break;
                            }

                            i += 2;
                            sb.Append((char)Convert.ToUInt16(hex.ToString(), 16));
                            break;
                        }
                        // 数字
                        case var digit when char.IsDigit(digit):
                        {
                            var j = 0;
                            var dec = new StringBuilder(3);
                            while (j < 3 && i + j < text.Length && char.IsDigit(text[i + j]))
                            {
                                dec.Append(text[i + j]);
                                j++;
                            }

                            i += j - 1;
                            sb.Append((char)Convert.ToUInt16(dec.ToString(), 10));
                            break;
                        }
                        case 'u':
                        {
                            // 解析 \u{xxxx} 形式的unicode字符
                            if (i + 2 >= text.Length)
                            {
                                tree.PushDiagnostic(new Diagnostic(DiagnosticSeverity.Error,
                                    DiagnosticCode.SyntaxError,
                                    "Unexpected end of string",
                                    new SourceRange(sourceRange.StartOffset + i - 1, 1)));
                                break;
                            }

                            var j = 1;
                            if (text[i + j] != '{')
                            {
                                tree.PushDiagnostic(new Diagnostic(DiagnosticSeverity.Error,
                                    DiagnosticCode.SyntaxError,
                                    $"Missing unicode escape sequence start '{{', current '{text[i + j]}'",
                                    new SourceRange(sourceRange.StartOffset + i + j, 1)));
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
                                    DiagnosticCode.SyntaxError,
                                    "Unexpected end of string",
                                    new SourceRange(sourceRange.StartOffset + i + j - 1, 1)));
                                break;
                            }

                            if (text[i + j] != '}')
                            {
                                tree.PushDiagnostic(new Diagnostic(DiagnosticSeverity.Error,
                                    DiagnosticCode.SyntaxError,
                                    $"Missing unicode escape sequence end '}}', current '{text[i + j]}'",
                                    new SourceRange(sourceRange.StartOffset + i + j, 1)));
                                break;
                            }

                            var unicodeHex = text[(i + 2)..(i + j)];
                            i += j;
                            if (unicodeHex.Length > 8)
                            {
                                tree.PushDiagnostic(new Diagnostic(DiagnosticSeverity.Error,
                                    DiagnosticCode.SyntaxError,
                                    $"Invalid unicode escape sequence '{unicodeHex}'",
                                    new SourceRange(sourceRange.StartOffset + i - j, unicodeHex.Length)));
                                break;
                            }

                            try
                            {
                                if (unicodeHex.Length == 0)
                                {
                                    tree.PushDiagnostic(new Diagnostic(DiagnosticSeverity.Error,
                                        DiagnosticCode.SyntaxError,
                                        $"Invalid unicode escape sequence '{unicodeHex}'",
                                        new SourceRange(sourceRange.StartOffset + i - j, unicodeHex.Length)));
                                    break;
                                }

                                var codePoint = Convert.ToInt32(unicodeHex.ToString(), 16);
                                if (codePoint > 0x10FFFF)
                                {
                                    tree.PushDiagnostic(new Diagnostic(DiagnosticSeverity.Error,
                                        DiagnosticCode.SyntaxError,
                                        $"Invalid unicode escape sequence '{unicodeHex}', the code point is too large",
                                        new SourceRange(sourceRange.StartOffset + i - j, unicodeHex.Length)));
                                    break;
                                }

                                sb.Append(char.ConvertFromUtf32(codePoint));
                            }
                            catch (OverflowException)
                            {
                                tree.PushDiagnostic(new Diagnostic(DiagnosticSeverity.Error,
                                    DiagnosticCode.SyntaxError,
                                    $"Invalid unicode escape sequence '{unicodeHex}', the code point is too large",
                                    new SourceRange(sourceRange.StartOffset + i - j, unicodeHex.Length)));
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
                            tree.PushDiagnostic(new Diagnostic(DiagnosticSeverity.Error,
                                DiagnosticCode.SyntaxError,
                                $"Invalid escape sequence '\\{text[i]}'",
                                new SourceRange(sourceRange.StartOffset + i, 1)));
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

        tree.SetStringTokenValue(index, sb.ToString());
    }

    // parse [[xxxx]]
    private static void CalculateLongString(int index, LuaSyntaxTree tree)
    {
        var sourceRange = tree.GetSourceRange(index);
        var text = tree.Document.Text.AsSpan(sourceRange.StartOffset, sourceRange.Length);
        if (text.Length < 4)
        {
            tree.SetStringTokenValue(index, string.Empty);
            return;
        }

        var equalNum = 0;
        var i = 0;
        if (text[i] != '[')
        {
            tree.PushDiagnostic(new Diagnostic(DiagnosticSeverity.Error,
                DiagnosticCode.SyntaxError,
                $"Invalid long string start, expected '[', current '{text[i]}'",
                new SourceRange(sourceRange.StartOffset, 1)));
            tree.SetStringTokenValue(index, string.Empty);
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
                DiagnosticCode.SyntaxError,
                $"Invalid long string start, expected '[', current '{text[i]}'",
                new SourceRange(sourceRange.StartOffset, 1)));
            tree.SetStringTokenValue(index, string.Empty);
        }

        i++;

        if (text.Length < i + equalNum + 2)
        {
            tree.PushDiagnostic(new Diagnostic(DiagnosticSeverity.Error,
                DiagnosticCode.SyntaxError,
                $"Invalid long string end, expected '{new string('=', equalNum)}]', current '{text[^1]}'",
                new SourceRange(sourceRange.StartOffset + text.Length - 1, 1)));
            tree.SetStringTokenValue(index, string.Empty);
        }

        var content = text[i..(text.Length - equalNum - 2)];

        tree.SetStringTokenValue(index, content.ToString());
    }

    private static void CalculateVersionNumber(int index, LuaSyntaxTree tree)
    {
        var sourceRange = tree.GetSourceRange(index);
        var text = tree.Document.Text.AsSpan(sourceRange.StartOffset, sourceRange.Length);
        if (text is "JIT")
        {
            tree.SetVersionNumber(index, LuaLanguageLevel.LuaJIT);
            return;
        }

        try
        {
            var version = VersionNumber.Parse(text.ToString());
            tree.SetVersionNumber(index, version);
        }
        catch (Exception e)
        {
            tree.PushDiagnostic(new Diagnostic(DiagnosticSeverity.Error,
                DiagnosticCode.SyntaxError,
                $"Invalid version number '{text}', {e.Message}",
                sourceRange));
            tree.SetVersionNumber(index, new VersionNumber(0, 0, 0, 0));
        }
    }

    private static void CalculateTemplateType(int index, LuaSyntaxTree tree)
    {
        var sourceRange = tree.GetSourceRange(index);
        var text = tree.Document.Text.AsSpan(sourceRange.StartOffset, sourceRange.Length);
        if (text.Length < 3)
        {
            tree.PushDiagnostic(new Diagnostic(DiagnosticSeverity.Error,
                DiagnosticCode.SyntaxError,
                "Invalid template type",
                sourceRange));
            tree.SetStringTokenValue(index, string.Empty);
        }

        var value = text[1..^1].ToString();
        tree.SetStringTokenValue(index, value);
    }
}
