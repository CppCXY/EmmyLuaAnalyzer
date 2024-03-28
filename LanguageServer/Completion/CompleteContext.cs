using EmmyLua.CodeAnalysis.Compilation.Semantic;
using EmmyLua.CodeAnalysis.Syntax.Node;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace LanguageServer.Completion;

public class CompleteContext(SemanticModel semanticModel, LuaSyntaxToken triggerToken, CancellationToken cancellationToken)
{
    public SemanticModel SemanticModel { get; } = semanticModel;
    
    public LuaSyntaxToken TriggerToken { get; } = triggerToken;
    
    private List<CompletionItem> Items { get; } = new();
    
    public IEnumerable<CompletionItem> CompletionItems => Items;
    
    public bool Continue { get; private set; }
    
    public void Add(CompletionItem item)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Items.Add(item);
    }
    
    public void StopHere()
    {
        Continue = false;
    }
}