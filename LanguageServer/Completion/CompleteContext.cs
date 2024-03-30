using System.Text;
using EmmyLua.CodeAnalysis.Compilation.Semantic;
using EmmyLua.CodeAnalysis.Compilation.Semantic.Render;
using EmmyLua.CodeAnalysis.Compilation.Type;
using EmmyLua.CodeAnalysis.Syntax.Node;
using EmmyLua.CodeAnalysis.Syntax.Tree;
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

    // ReSharper disable once ConvertToPrimaryConstructor
    public CompleteContext(SemanticModel semanticModel, Position position, CancellationToken cancellationToken)
    {
        SemanticModel = semanticModel;
        Position = position;
        Continue = true;
        CancellationToken = cancellationToken;
        TriggerToken =
            semanticModel.Document.SyntaxTree.SyntaxRoot.TokenLeftBiasedAt(position.Line, position.Character);
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
}