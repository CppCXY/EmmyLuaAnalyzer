using EmmyLua.CodeAnalysis.Compilation.Semantic;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;
using LanguageServer.Util;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using Range = OmniSharp.Extensions.LanguageServer.Protocol.Models.Range;

namespace LanguageServer.InlineValues;

public class InlineValuesBuilder
{
    public List<InlineValueBase> Build(SemanticModel semanticModel, Range range, InlineValueContext context)
    {
        var sourceRange = range.ToSourceRange(semanticModel.Document);

        var result = new List<InlineValueBase>();
        foreach (var node in semanticModel.Document.SyntaxTree.SyntaxRoot.DescendantsInRange(sourceRange))
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