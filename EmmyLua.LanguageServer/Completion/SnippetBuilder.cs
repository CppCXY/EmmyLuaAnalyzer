using EmmyLua.LanguageServer.Framework.Protocol.Message.Completion;
using EmmyLua.LanguageServer.Framework.Protocol.Model.Kind;
using EmmyLua.LanguageServer.Framework.Protocol.Model.TextEdit;
using EmmyLua.LanguageServer.Framework.Protocol.Model.Union;

namespace EmmyLua.LanguageServer.Completion;

public class SnippetBuilder(string label, CompleteContext completeContext)
{
    private string Label { get; set; } = label;

    private string? Detail { get; set; }

    private string? Description { get; set; }
    
    private string? InsertText { get; set; }

    private TextEditOrInsertReplaceEdit? TextEdit { get; set; }

    private List<AnnotatedTextEdit>? AdditionalTextEdit { get; set; }

    public SnippetBuilder WithDetail(string detail)
    {
        Detail = detail;
        return this;
    }

    public SnippetBuilder WithDescription(string description)
    {
        Description = description;
        return this;
    }

    public SnippetBuilder WithTextEdit(TextEditOrInsertReplaceEdit textEdit)
    {
        TextEdit = textEdit;
        return this;
    }

    public SnippetBuilder WithInsertText(string text)
    {
        InsertText = text;
        return this;
    }

    public SnippetBuilder WithAdditionalTextEdit(AnnotatedTextEdit textEdit)
    {
        AdditionalTextEdit = [textEdit];
        return this;
    }

    public void AddToContext()
    {
        completeContext.Add(new CompletionItem()
        {
            Label = Label,
            LabelDetails = new CompletionItemLabelDetails()
            {
                Detail = Detail,
                Description = Description
            },
            InsertText = InsertText,
            InsertTextMode = InsertTextMode.AdjustIndentation,
            TextEdit = TextEdit,
            AdditionalTextEdits = AdditionalTextEdit,
            Kind = CompletionItemKind.Event,
            InsertTextFormat = InsertTextFormat.Snippet,
        });
    }
}