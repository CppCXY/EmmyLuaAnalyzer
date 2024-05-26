using EmmyLua.CodeAnalysis.Compilation.Semantic;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;
using EmmyLua.LanguageServer.Util;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using Range = OmniSharp.Extensions.LanguageServer.Protocol.Models.Range;

namespace EmmyLua.LanguageServer.InlineValues;

public class InlineValuesBuilder
{
    public List<InlineValueBase> Build(SemanticModel semanticModel, Range range, InlineValueContext context,
        CancellationToken cancellationToken)
    {
        var result = new List<InlineValueBase>();
        var stopRange = context.StoppedLocation;
        var token = semanticModel.Document.SyntaxTree.SyntaxRoot.TokenAt(stopRange.End.Line, stopRange.End.Character);
        if (token is null)
        {
            return result;
        }

        var baseRange = token.Ancestors.OfType<LuaClosureExprSyntax>().FirstOrDefault()?.Range ??
                        range.ToSourceRange(semanticModel.Document);
        var stopOffset = semanticModel.Document.GetOffset(stopRange.End.Line, stopRange.End.Character);
        if (baseRange.StartOffset < stopOffset)
        {
            baseRange = baseRange with { Length = stopOffset - baseRange.StartOffset };
        }

        if (cancellationToken.IsCancellationRequested)
        {
            return result;
        }

        foreach (var node in semanticModel.Document.SyntaxTree.SyntaxRoot.DescendantsInRange(baseRange))
        {
            switch (node)
            {
                case LuaLocalNameSyntax { Name: { } localName }:
                {
                    result.Add(new InlineValueVariableLookup
                    {
                        VariableName = localName.RepresentText,
                        Range = localName.Range.ToLspRange(semanticModel.Document)
                    });
                    break;
                }
                case LuaParamDefSyntax { Name: { } paramName }:
                {
                    result.Add(new InlineValueVariableLookup
                    {
                        VariableName = paramName.RepresentText,
                        Range = paramName.Range.ToLspRange(semanticModel.Document)
                    });
                    break;
                }
                case LuaNameExprSyntax { Name: { } name }:
                {
                    result.Add(new InlineValueVariableLookup
                    {
                        VariableName = name.RepresentText,
                        Range = name.Range.ToLspRange(semanticModel.Document)
                    });
                    break;
                }
            }
        }

        return result;
    }
}