using System.Globalization;
using EmmyLua.CodeAnalysis.Compilation.Semantic;
using EmmyLua.CodeAnalysis.Document;
using EmmyLua.CodeAnalysis.Kind;
using EmmyLua.CodeAnalysis.Syntax.Node;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;
using LanguageServer.Util;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace LanguageServer.DocumentColor;

public class DocumentColorBuilder
{
    public List<ColorInformation> Build(SemanticModel semanticModel)
    {
        var colors = new List<ColorInformation>();
        var stringTokens = semanticModel
            .Document.SyntaxTree.SyntaxRoot.DescendantsWithToken
            .OfType<LuaSyntaxToken>();

        foreach (var token in stringTokens)
        {
            if (token is { Kind: LuaTokenKind.TkString or LuaTokenKind.TkDocDetail })
            {
                CheckColor(token, colors, semanticModel);
            }
        }

        return colors;
    }

    private void CheckColor(LuaSyntaxToken token, List<ColorInformation> colors, SemanticModel semanticModel)
    {
        var text = token.RepresentText;
        var pos = 0;

        while ((pos = text.IndexOf('#', pos)) != -1)
        {
            var start = pos;
            var end = pos + 7;
            if (end > text.Length)
            {
                return;
            }

            var sourceRange = new SourceRange()
            {
                StartOffset = token.Range.StartOffset + start,
                Length = 7
            };

            var color = text.Substring(start, 7);

            try
            {
                colors.Add(new ColorInformation()
                {
                    Range = sourceRange.ToLspRange(semanticModel.Document),
                    Color = new OmniSharp.Extensions.LanguageServer.Protocol.Models.DocumentColor()
                    {
                        Red = int.Parse(color.Substring(1, 2), NumberStyles.HexNumber) / 255.0,
                        Green = int.Parse(color.Substring(3, 2), NumberStyles.HexNumber) / 255.0,
                        Blue = int.Parse(color.Substring(5, 2), NumberStyles.HexNumber) / 255.0,
                        Alpha = 1
                    }
                });
            }
            catch (Exception)
            {
                // ignored
            }

            pos = end;
        }
    }

    public List<ColorPresentation> ModifyColor(ColorInformation info, SemanticModel semanticModel)
    {
        var color = info.Color;
        var colorPresentations = new List<ColorPresentation>();
        var r = (int) (color.Red * 255);
        var g = (int) (color.Green * 255);
        var b = (int) (color.Blue * 255);
        
        
        var colorPresentation = new ColorPresentation()
        {
            Label = $"#{r:X2}{g:X2}{b:X2}",
            TextEdit = new TextEdit()
            {
                Range = info.Range,
                NewText = $"#{r:X2}{g:X2}{b:X2}"
            }
        };
        colorPresentations.Add(colorPresentation);
        return colorPresentations;
    } 
}