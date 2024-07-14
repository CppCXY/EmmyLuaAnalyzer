using EmmyLua.CodeAnalysis.Compilation.Semantic;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;
using EmmyLua.LanguageServer.Framework.Protocol.Message.InlineValue;
using EmmyLua.LanguageServer.Framework.Protocol.Model;
using EmmyLua.LanguageServer.Util;

namespace EmmyLua.LanguageServer.InlineValues;

public class InlineValuesBuilder
{
    public List<InlineValue> Build(SemanticModel semanticModel, DocumentRange range)
    {
        var result = new List<InlineValue>();
        var baseRange = range.ToSourceRange(semanticModel.Document);
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