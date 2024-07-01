using System.Globalization;
using EmmyLua.CodeAnalysis.Compilation.Semantic;
using EmmyLua.CodeAnalysis.Document;
using EmmyLua.CodeAnalysis.Syntax.Kind;
using EmmyLua.CodeAnalysis.Syntax.Node;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;
using EmmyLua.LanguageServer.Util;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace EmmyLua.LanguageServer.DocumentColor;

public class DocumentColorBuilder
{
    public List<ColorInformation> Build(SemanticModel semanticModel)
    {
        var colors = new List<ColorInformation>();
        var stringTokens = semanticModel
            .Document.SyntaxTree.SyntaxRoot.DescendantsWithToken
            .OfType<LuaStringToken>();

        foreach (var token in stringTokens)
        {
            CheckColor(token, colors, semanticModel);
        }

        return colors;
    }

    private void CheckColor(LuaStringToken token, List<ColorInformation> colors, SemanticModel semanticModel)
    {
        var text = token.Text;

        for (var i = 0; i < text.Length; i++)
        {
            if (char.IsAsciiHexDigit(text[i]))
            {
                var start = i;
                var length = 1;
                while (i + length < text.Length && char.IsAsciiHexDigit(text[i + length]))
                {
                    length++;
                }

                if (length is 6 or 8)
                {
                    // 判断是否为单词边界
                    if (start > 0 && char.IsLetterOrDigit(text[start - 1]))
                    {
                        continue;
                    }

                    if (start + length < text.Length && char.IsLetterOrDigit(text[start + length]))
                    {
                        continue;
                    }


                    var range = new SourceRange()
                    {
                        StartOffset = token.Range.StartOffset + start,
                        Length = length
                    };
                    AddColorInformation(text[start..(start + length)], range, colors, semanticModel);
                }

                i += length;
            }
        }
    }

    void AddColorInformation(ReadOnlySpan<char> colorText, SourceRange range, List<ColorInformation> colors,
        SemanticModel semanticModel)
    {
        if (colorText.Length == 6)
        {
            var red = int.Parse(colorText.Slice(0, 2), NumberStyles.HexNumber) / 255.0f;
            var green = int.Parse(colorText.Slice(2, 2), NumberStyles.HexNumber) / 255.0f;
            var blue = int.Parse(colorText.Slice(4, 2), NumberStyles.HexNumber) / 255.0f;

            colors.Add(new ColorInformation()
            {
                Range = range.ToLspRange(semanticModel.Document),
                Color = new OmniSharp.Extensions.LanguageServer.Protocol.Models.DocumentColor()
                {
                    Red = red,
                    Green = green,
                    Blue = blue,
                    Alpha = 1
                }
            });
        }
        else if (colorText.Length == 8)
        {
            var red = int.Parse(colorText.Slice(0, 2), NumberStyles.HexNumber) / 255.0f;
            var green = int.Parse(colorText.Slice(2, 2), NumberStyles.HexNumber) / 255.0f;
            var blue = int.Parse(colorText.Slice(4, 2), NumberStyles.HexNumber) / 255.0f;
            var alpha = int.Parse(colorText.Slice(6, 2), NumberStyles.HexNumber) / 255.0f;

            colors.Add(new ColorInformation()
            {
                Range = range.ToLspRange(semanticModel.Document),
                Color = new OmniSharp.Extensions.LanguageServer.Protocol.Models.DocumentColor()
                {
                    Red = red,
                    Green = green,
                    Blue = blue,
                    Alpha = alpha
                }
            });
        }
    }

    public List<ColorPresentation> ModifyColor(ColorInformation info, SemanticModel semanticModel)
    {
        var range = info.Range;
        var rangeLength = 0;
        if (range is { Start: { } start, End: { } end })
        {
            rangeLength = end.Character - start.Character;
        }

        var color = info.Color;
        var colorPresentations = new List<ColorPresentation>();
        var r = (int)(color.Red * 255);
        var g = (int)(color.Green * 255);
        var b = (int)(color.Blue * 255);
        var a = (int)(color.Alpha * 255);

        var newText = rangeLength == 6 ? $"{r:X2}{g:X2}{b:X2}" : $"{r:X2}{g:X2}{b:X2}{a:X2}";
        var colorPresentation = new ColorPresentation()
        {
            Label = $"{r:X2}{g:X2}{b:X2}",
            TextEdit = new TextEdit()
            {
                Range = info.Range,
                NewText = newText
            }
        };
        colorPresentations.Add(colorPresentation);
        return colorPresentations;
    }
}