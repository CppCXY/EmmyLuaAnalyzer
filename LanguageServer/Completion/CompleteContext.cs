using EmmyLua.CodeAnalysis.Compilation.Semantic;
using EmmyLua.CodeAnalysis.Compilation.Type;
using EmmyLua.CodeAnalysis.Syntax.Node;
using EmmyLua.Configuration;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace LanguageServer.Completion;

public class CompleteContext
{
    public SemanticModel SemanticModel { get; }

    public LuaSyntaxToken? TriggerToken { get; }

    public Position Position { get; }

    private List<CompletionItem> Items { get; } = new();

    public IEnumerable<CompletionItem> CompletionItems => Items;

    public bool Continue { get; private set; }

    private CancellationToken CancellationToken { get; }
    
    public Setting Setting { get; }

    // ReSharper disable once ConvertToPrimaryConstructor
    public CompleteContext(SemanticModel semanticModel, Position position, CancellationToken cancellationToken, Setting setting)
    {
        SemanticModel = semanticModel;
        Position = position;
        Continue = true;
        CancellationToken = cancellationToken;
        TriggerToken =
            semanticModel.Document.SyntaxTree.SyntaxRoot.TokenLeftBiasedAt(position.Line, position.Character);
        Setting = setting;
    }

    public void Add(CompletionItem item)
    {
        CancellationToken.ThrowIfCancellationRequested();
        Items.Add(item);
    }
    
    public void AddRange(IEnumerable<CompletionItem> items)
    {
        CancellationToken.ThrowIfCancellationRequested();
        Items.AddRange(items);
    }
    
    public void StopHere()
    {
        Continue = false;
    }

    public CompletionItemBuilder CreateCompletion(string label, LuaType? type)
    {
        return new CompletionItemBuilder(label, type ?? Builtin.Any, this);
    }
}