using EmmyLua.CodeAnalysis.Kind;
using EmmyLua.CodeAnalysis.Syntax.Node;
using LanguageServer.Util;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace LanguageServer.Completion.CompleteProvider;

public class PostfixProvider : ICompleteProviderBase
{
    public void AddCompletion(CompleteContext context)
    {
        var trigger = context.TriggerToken;
        if (trigger is { Kind: LuaTokenKind.TkAt })
        {
            var leftPos = trigger.Position - 1;
            var paramToken = context.SemanticModel.Document.SyntaxTree.SyntaxRoot.TokenAt(leftPos);
            if (paramToken?.Parent is LuaSyntaxNode node)
            {
                AddPostfixCompletion(context, node);
            }
        }
    }


    private void AddPostfixCompletion(CompleteContext context, LuaSyntaxNode paramNode)
    {
        var document = paramNode.Tree.Document;
        var paramRange = paramNode.Range;
        // var triggerRange = context.TriggerToken!.Range;
        var replaceRange = (paramRange with { Length = paramRange.Length + 1 }).ToLspRange(document);

        void AddSnippet(string label, string text)
        {
            context.CreateSnippet(label)
                .WithInsertText(text)
                .WithAdditionalTextEdit(new TextEdit()
                {
                    NewText = string.Empty,
                    Range = replaceRange,
                })
                .AddToContext();
        }

        var paramText = paramNode.Tree.Document.Text[paramRange.StartOffset..paramRange.EndOffset];
        AddSnippet("if", $"if {paramText}$1 then\n\t$0\nend");
        AddSnippet("while", $"while {paramText}$1 do\n\t$0\nend");
        AddSnippet("forp", $"for ${{2:k}} in pairs({paramText}$1) do\n\t$0\nend");
        AddSnippet("fori", $"for ${{1:i}} = ${{2:1}}, #{paramText} do\n\t$0\nend");
        AddSnippet("function", $"function {paramText}${{1:func}}(${{2:...}})\n\t$0\nend");
        AddSnippet("insert", $"table.insert({paramText}, $1)");
        AddSnippet("remove", $"table.remove({paramText}, $1)");
        AddSnippet("++", $"{paramText} = {paramText} + 1");
        AddSnippet("--", $"{paramText} = {paramText} - 1");
        AddSnippet("+n", $"{paramText} = {paramText} + $1");
    }
}