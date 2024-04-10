using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace LanguageServer.Completion.CompleteProvider;

public class KeywordsProvider : ICompleteProviderBase
{
    private List<CompletionItem> Keywords { get; } = new()
    {
        new CompletionItem() { Label = "if", Kind = CompletionItemKind.Keyword, Detail = "keyword" },
        new CompletionItem() { Label = "else", Kind = CompletionItemKind.Keyword, Detail = "keyword" },
        new CompletionItem() { Label = "elseif", Kind = CompletionItemKind.Keyword, Detail = "keyword" },
        new CompletionItem() { Label = "then", Kind = CompletionItemKind.Keyword, Detail = "keyword" },
        new CompletionItem() { Label = "end", Kind = CompletionItemKind.Keyword, Detail = "keyword" },
        new CompletionItem() { Label = "for", Kind = CompletionItemKind.Keyword, Detail = "keyword" },
        new CompletionItem() { Label = "in", Kind = CompletionItemKind.Keyword, Detail = "keyword" },
        new CompletionItem() { Label = "do", Kind = CompletionItemKind.Keyword, Detail = "keyword" },
        new CompletionItem() { Label = "while", Kind = CompletionItemKind.Keyword, Detail = "keyword" },
        new CompletionItem() { Label = "repeat", Kind = CompletionItemKind.Keyword, Detail = "keyword" },
        new CompletionItem() { Label = "until", Kind = CompletionItemKind.Keyword, Detail = "keyword" },
        new CompletionItem() { Label = "break", Kind = CompletionItemKind.Keyword, Detail = "keyword" },
        new CompletionItem() { Label = "return", Kind = CompletionItemKind.Keyword, Detail = "keyword" },
        new CompletionItem() { Label = "function", Kind = CompletionItemKind.Keyword, Detail = "keyword" },
        new CompletionItem() { Label = "local", Kind = CompletionItemKind.Keyword, Detail = "keyword" },
        new CompletionItem() { Label = "nil", Kind = CompletionItemKind.Constant, Detail = "nil" },
        new CompletionItem() { Label = "true", Kind = CompletionItemKind.Constant, Detail = "true" },
        new CompletionItem() { Label = "false", Kind = CompletionItemKind.Constant, Detail = "false" },
        new CompletionItem() { Label = "and", Kind = CompletionItemKind.Keyword, Detail = "keyword" },
        new CompletionItem() { Label = "or", Kind = CompletionItemKind.Keyword, Detail = "keyword" },
        new CompletionItem() { Label = "not", Kind = CompletionItemKind.Keyword, Detail = "keyword" },
        new CompletionItem() { Label = "goto", Kind = CompletionItemKind.Keyword, Detail = "keyword" },
    };

    public void AddCompletion(CompleteContext context)
    {
        if (context.TriggerToken?.Parent is not LuaNameExprSyntax)
        {
            return;
        }

        context.AddRange(Keywords);
    }
}