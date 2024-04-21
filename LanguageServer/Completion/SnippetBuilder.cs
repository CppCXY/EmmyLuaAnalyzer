using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace LanguageServer.Completion;

public class SnippetBuilder(string label, CompleteContext completeContext)
{
    private string Label { get; set; } = label;

    private string? Detail { get; set; }

    private string? Description { get; set; }
    
    private string? InsertText { get; set; }

    private TextEditOrInsertReplaceEdit? TextEdit { get; set; }

    private TextEditContainer? AdditionalTextEdit { get; set; }

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

    public SnippetBuilder WithAdditionalTextEdit(TextEdit textEdit)
    {
        AdditionalTextEdit = new TextEditContainer(textEdit);
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