using EmmyLua.CodeAnalysis.Compilation.Semantic;
using EmmyLua.CodeAnalysis.Compilation.Semantic.Render;
using EmmyLua.CodeAnalysis.Syntax.Node;
using EmmyLua.CodeAnalysis.Type;
using EmmyLua.LanguageServer.Server;
using EmmyLua.LanguageServer.Util;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace EmmyLua.LanguageServer.Completion;

public class CompleteContext
{
    public SemanticModel SemanticModel { get; }

    public LuaSyntaxToken? TriggerToken { get; }

    public Position Position { get; }

    private List<CompletionItem> Items { get; } = new();

    public IEnumerable<CompletionItem> CompletionItems => Items;

    public bool Continue { get; private set; }

    private CancellationToken CancellationToken { get; }

    public CompletionConfig CompletionConfig { get; }
    
    public ServerContext ServerContext { get; }
    
    public LuaRenderFeature RenderFeature { get; } = new(
        true,
        false,
        false,
        100
    );

    // ReSharper disable once ConvertToPrimaryConstructor
    public CompleteContext(SemanticModel semanticModel, Position position, CancellationToken cancellationToken,
        ServerContext context)
    {
        SemanticModel = semanticModel;
        Position = position;
        Continue = true;
        CancellationToken = cancellationToken;
        TriggerToken =
            semanticModel.Document.SyntaxTree.SyntaxRoot.TokenLeftBiasedAt(position.Line, position.Character);
        CompletionConfig = context.SettingManager.GetCompletionConfig();
        ServerContext = context;
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
    
    public SnippetBuilder CreateSnippet(string label)
    {
        return new SnippetBuilder(label, this);
    }
}