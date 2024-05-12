using EmmyLua.CodeAnalysis.Kind;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;
using EmmyLua.LanguageServer.Completion.CompletionData;
using EmmyLua.LanguageServer.ExecuteCommand.Commands;
using EmmyLua.LanguageServer.Util;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace EmmyLua.LanguageServer.Completion.CompleteProvider;

public class KeywordsProvider : ICompleteProviderBase
{
    public void AddCompletion(CompleteContext context)
    {
        if (context.TriggerToken?.Parent is not LuaNameExprSyntax nameExpr)
        {
            return;
        }

        if (nameExpr.Parent is not LuaBlockSyntax)
        {
            return;
        }

        context.AddRange(KeySnippets.Keywords);
        ContinueCompletion(context);
    }

    private void ContinueCompletion(CompleteContext context)
    {
        var triggerToken = context.TriggerToken;
        var stats = triggerToken!.Ancestors.OfType<LuaStatSyntax>();
        foreach (var stat in stats)
        {
            if (stat is LuaForStatSyntax or LuaForRangeStatSyntax or LuaWhileStatSyntax)
            {
                context.Add(new CompletionItem()
                {
                    Label = "continue",
                    Kind = CompletionItemKind.Keyword,
                    LabelDetails = new()
                    {
                        Detail = " (goto continue)"
                    },
                    InsertTextMode = InsertTextMode.AdjustIndentation,
                    InsertText = "goto continue",
                    AdditionalTextEdits = GetContinueLabelTextEdit(stat) is { } textEdit
                        ? new TextEditContainer(textEdit)
                        : null
                });
                break;
            }
        }
    }

    private TextEdit? GetContinueLabelTextEdit(LuaStatSyntax loopStat)
    {
        var endToken = loopStat.FirstChildToken(LuaTokenKind.TkEnd);
        if (endToken is not null)
        {
            var document = loopStat.Tree.Document;
            var blockIndentText = string.Empty;
            if (loopStat.FirstChild<LuaBlockSyntax>()?.StatList.LastOrDefault() is { } lastStat)
            {
                var indentToken = lastStat.GetPrevSibling();
                if (indentToken is LuaWhitespaceToken
                    {
                        RepresentText: { } indentText2
                    })
                {
                    blockIndentText = indentText2;
                }
            }

            var endIndentText = string.Empty;
            if (endToken.GetPrevToken() is LuaWhitespaceToken
                {
                    RepresentText: { } indentText
                })
            {
                endIndentText = indentText;
            }

            if (blockIndentText.Length > 0 && endIndentText.Length > 0 && blockIndentText.Length > endIndentText.Length)
            {
                blockIndentText = blockIndentText[endIndentText.Length..];
            }

            var newText = $"{blockIndentText}::continue::\n{endIndentText}end";
            return new TextEdit()
            {
                Range = endToken.Range.ToLspRange(document),
                NewText = newText
            };
        }

        return null;
    }
}