using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace EmmyLua.LanguageServer.Completion.CompletionData;

public static class KeySnippets
{
    public static List<CompletionItem> StatKeyWords { get; }=
    [
        new CompletionItem()
        {
            Label = "if",
            Kind = CompletionItemKind.Snippet,
            InsertTextMode = InsertTextMode.AdjustIndentation,
            InsertText = "if ${1:condition} then\n\t${0}\nend",
            LabelDetails = new()
            {
                Detail = " (if condition then ... end)"
            },
            InsertTextFormat = InsertTextFormat.Snippet
        },
        new CompletionItem()
        {
            Label = "else",
            Kind = CompletionItemKind.Snippet,
            InsertTextMode = InsertTextMode.AdjustIndentation,
            InsertText = "else\n\t${0}",
            LabelDetails = new()
            {
                Detail = " (else ...)"
            },
            InsertTextFormat = InsertTextFormat.Snippet
        },
        new CompletionItem()
        {
            Label = "elseif",
            Kind = CompletionItemKind.Snippet,
            InsertTextMode = InsertTextMode.AdjustIndentation,
            InsertText = "elseif ${1:condition} then\n\t${0}",
            LabelDetails = new()
            {
                Detail = " (elseif condition then ... )"
            },
            InsertTextFormat = InsertTextFormat.Snippet
        },
        new CompletionItem()
        {
            Label = "then",
            Kind = CompletionItemKind.Snippet,
            InsertTextMode = InsertTextMode.AdjustIndentation,
            InsertText = "then\n\t${0}",
            LabelDetails = new()
            {
                Detail = " (then ... )"
            },
            InsertTextFormat = InsertTextFormat.Snippet
        },
        new CompletionItem() { Label = "end", Kind = CompletionItemKind.Keyword, Detail = "keyword" },
        new CompletionItem()
        {
            Label = "fori",
            Kind = CompletionItemKind.Snippet,
            InsertTextMode = InsertTextMode.AdjustIndentation,
            InsertText = "for ${1:i} = ${2:1}, ${3:finish} do\n\t${0}\nend",
            LabelDetails = new()
            {
                Detail = " (for i = 1, finish do ... end)"
            },
            InsertTextFormat = InsertTextFormat.Snippet
        },
        new CompletionItem()
        {
            Label = "forp",
            Kind = CompletionItemKind.Snippet,
            InsertTextMode = InsertTextMode.AdjustIndentation,
            InsertText = "for ${1:k}, ${2:v} in pairs(${3:table}) do\n\t${0}\nend",
            LabelDetails = new()
            {
                Detail = " (for k,v in pairs(table) do ... end)"
            },
            InsertTextFormat = InsertTextFormat.Snippet
        },
        new CompletionItem()
        {
            Label = "forip",
            Kind = CompletionItemKind.Snippet,
            InsertTextMode = InsertTextMode.AdjustIndentation,
            InsertText = "for ${1:i},${2:v} in ipairs(${3:table}) do\n\t${0}\nend",
            LabelDetails = new()
            {
                Detail = " (for i, v in ipairs(table) do ... end)"
            },
            InsertTextFormat = InsertTextFormat.Snippet
        },
        new CompletionItem()
        {
            Label = "in",
            Kind = CompletionItemKind.Snippet,
            InsertTextMode = InsertTextMode.AdjustIndentation,
            InsertText = "in pairs(${1:table}) do\n\t${0}\nend",
            LabelDetails = new()
            {
                Detail = " (in pairs(table) do ... end)"
            },
            InsertTextFormat = InsertTextFormat.Snippet
        },
        new CompletionItem()
        {
            Label = "do",
            Kind = CompletionItemKind.Snippet,
            InsertTextMode = InsertTextMode.AdjustIndentation,
            InsertText = "do\n\t${0}\nend",
            LabelDetails = new()
            {
                Detail = " (do ... end)"
            },
            InsertTextFormat = InsertTextFormat.Snippet
        },
        new CompletionItem()
        {
            Label = "while",
            Kind = CompletionItemKind.Snippet,
            InsertTextMode = InsertTextMode.AdjustIndentation,
            InsertText = "while ${1:condition} do\n\t${0}\nend",
            LabelDetails = new()
            {
                Detail = " (while condition do ... end)"
            },
            InsertTextFormat = InsertTextFormat.Snippet
        },
        new CompletionItem()
        {
            Label = "repeat",
            Kind = CompletionItemKind.Snippet,
            InsertTextMode = InsertTextMode.AdjustIndentation,
            InsertText = "repeat\n\t${0}\nuntil ${1:condition}",
            LabelDetails = new()
            {
                Detail = " (repeat ... until condition)"
            },
            InsertTextFormat = InsertTextFormat.Snippet
        },
        new CompletionItem()
        {
            Label = "until",
            Kind = CompletionItemKind.Snippet,
            InsertTextMode = InsertTextMode.AdjustIndentation,
            InsertText = "until ${1:condition}",
            LabelDetails = new()
            {
                Detail = " (until condition)"
            },
            InsertTextFormat = InsertTextFormat.Snippet
        },
        new CompletionItem() { Label = "break", Kind = CompletionItemKind.Keyword, Detail = "keyword" },
        new CompletionItem() { Label = "return", Kind = CompletionItemKind.Keyword, Detail = "keyword" },
        new CompletionItem()
        {
            Label = "function",
            Kind = CompletionItemKind.Snippet,
            InsertTextMode = InsertTextMode.AdjustIndentation,
            InsertText = "function ${1:name}(${2:...})\n\t${0}\nend",
            LabelDetails = new()
            {
                Detail = " (function name(...) ... end)"
            },
            InsertTextFormat = InsertTextFormat.Snippet
        },
        new CompletionItem() { Label = "local", Kind = CompletionItemKind.Keyword, Detail = "keyword" },
        new CompletionItem()
        {
            Label = "local function",
            Kind = CompletionItemKind.Snippet,
            InsertTextMode = InsertTextMode.AdjustIndentation,
            InsertText = "local function ${1:name}(${2:...})\n\t${0}\nend",
            LabelDetails = new()
            {
                Detail = " (local function name(...) ... end)"
            },
            InsertTextFormat = InsertTextFormat.Snippet
        },
    ];

    public static List<CompletionItem> ExprKeywords { get; } =
    [
        new CompletionItem()
        {
            Label = "function",
            Kind = CompletionItemKind.Snippet,
            InsertTextMode = InsertTextMode.AdjustIndentation,
            InsertText = "function (${1:...})\n\t${0}\nend",
            LabelDetails = new()
            {
                Detail = " (function (...) ... end)"
            },
            InsertTextFormat = InsertTextFormat.Snippet
        },
        new CompletionItem() { Label = "nil", Kind = CompletionItemKind.Constant, Detail = "nil" },
        new CompletionItem() { Label = "true", Kind = CompletionItemKind.Constant, Detail = "true" },
        new CompletionItem() { Label = "false", Kind = CompletionItemKind.Constant, Detail = "false" },
        new CompletionItem() { Label = "and", Kind = CompletionItemKind.Keyword, Detail = "keyword" },
        new CompletionItem()
        {
            Label = "and or",
            Kind = CompletionItemKind.Snippet,
            InsertTextMode = InsertTextMode.AdjustIndentation,
            InsertText = "and ${1:result1} or ${2:result2}",
            LabelDetails = new()
            {
                Detail = " (and result1 or result2)"
            },
            InsertTextFormat = InsertTextFormat.Snippet
        },
        new CompletionItem() { Label = "or", Kind = CompletionItemKind.Keyword, Detail = "keyword" },
        new CompletionItem() { Label = "not", Kind = CompletionItemKind.Keyword, Detail = "keyword" },
        new CompletionItem() { Label = "goto", Kind = CompletionItemKind.Keyword, Detail = "keyword" },
    ];
}