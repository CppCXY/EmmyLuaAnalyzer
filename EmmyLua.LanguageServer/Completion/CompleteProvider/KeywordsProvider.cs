using EmmyLua.CodeAnalysis.Syntax.Kind;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;
using EmmyLua.LanguageServer.Completion.CompletionData;
using EmmyLua.LanguageServer.Framework.Protocol.Message.Completion;
using EmmyLua.LanguageServer.Framework.Protocol.Model.Kind;
using EmmyLua.LanguageServer.Framework.Protocol.Model.TextEdit;
using EmmyLua.LanguageServer.Util;

namespace EmmyLua.LanguageServer.Completion.CompleteProvider;

public class KeywordsProvider : ICompleteProviderBase
{
    public void AddCompletion(CompleteContext context)
    {
        if (context.TriggerToken?.Parent is not LuaNameExprSyntax nameExpr)
        {
            return;
        }

        if (nameExpr.Parent?.Parent is LuaBlockSyntax)
        {
            context.AddRange(KeySnippets.StatKeyWords);
            AddSmartFuncStatCompletion(context);
        }

        context.AddRange(KeySnippets.ExprKeywords);
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
                        ? [textEdit]
                        : null
                });
                break;
            }
        }
    }

    private AnnotatedTextEdit? GetContinueLabelTextEdit(LuaStatSyntax loopStat)
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
            return new()
            {
                Range = endToken.Range.ToLspRange(document),
                NewText = newText
            };
        }

        return null;
    }

    private void AddSmartFuncStatCompletion(CompleteContext context)
    {
        var prevStat = context.TriggerToken?.Parent?.Parent?.PrevOfType<LuaStatSyntax>().FirstOrDefault();
        if (prevStat is not LuaFuncStatSyntax funcStatSyntax)
        {
            return;
        }

        var indexExpr = funcStatSyntax.IndexExpr;
        if (indexExpr is { PrefixExpr.Text: { } text, IsColonIndex: { } colonIndex })
        {
            var dot = colonIndex ? ":" : ".";
            
            context.CreateSnippet("function")
                .WithInsertText($"function {text}{dot}${{1:name}}(${{2:...}})\n\t${0}\nend")
                .WithDetail($" (function {text}{dot}name(...) .. end)")
                .AddToContext();
        }
    }
}