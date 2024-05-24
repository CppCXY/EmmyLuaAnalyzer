using EmmyLua.CodeAnalysis.Compilation.Analyzer;
using EmmyLua.CodeAnalysis.Compilation.Analyzer.DeclarationAnalyzer;
using EmmyLua.CodeAnalysis.Compilation.Analyzer.FlowAnalyzer;
using EmmyLua.CodeAnalysis.Compilation.Analyzer.ResolveAnalyzer;
using EmmyLua.CodeAnalysis.Compilation.Declaration;
using EmmyLua.CodeAnalysis.Compilation.Index;
using EmmyLua.CodeAnalysis.Compilation.Infer;
using EmmyLua.CodeAnalysis.Compilation.Semantic;
using EmmyLua.CodeAnalysis.Diagnostics;
using EmmyLua.CodeAnalysis.Document;
using EmmyLua.CodeAnalysis.Syntax.Tree;
using EmmyLua.CodeAnalysis.Workspace;

namespace EmmyLua.CodeAnalysis.Compilation;

public class LuaCompilation
{
    public LuaWorkspace Workspace { get; }

    private readonly Dictionary<LuaDocumentId, LuaSyntaxTree> _syntaxTrees = new();

    public IEnumerable<LuaSyntaxTree> SyntaxTrees => _syntaxTrees.Values;

    public DbManager Db { get; }

    private HashSet<LuaDocumentId> DirtyDocumentIds { get; } = [];

    internal Dictionary<LuaDocumentId, LuaDeclarationTree> DeclarationTrees { get; } = new();

    private List<LuaAnalyzer> Analyzers { get; }

    public LuaDiagnostics Diagnostics { get; }

    public LuaCompilation(LuaWorkspace workspace)
    {
        Workspace = workspace;
        Db = new DbManager(this);
        Analyzers =
        [
            new DeclarationAnalyzer(this),
            new FlowAnalyzer(this),
            new ResolveAnalyzer(this),
        ];
        Diagnostics = new LuaDiagnostics(this);
    }

    private void InternalAddSyntaxTree(LuaDocumentId documentId, LuaSyntaxTree syntaxTree)
    {
        if (!_syntaxTrees.TryAdd(documentId, syntaxTree))
        {
            RemoveCache(documentId);
            _syntaxTrees[documentId] = syntaxTree;
        }

        AddDirtyDocument(documentId);
    }

    public void AddSyntaxTrees(IEnumerable<(LuaDocumentId, LuaSyntaxTree)> syntaxTrees)
    {
        foreach (var (documentId, syntaxTree) in syntaxTrees)
        {
            InternalAddSyntaxTree(documentId, syntaxTree);
        }

        Analyze();
    }

    public void AddSyntaxTree(LuaDocumentId documentId, LuaSyntaxTree syntaxTree)
    {
        InternalAddSyntaxTree(documentId, syntaxTree);
        Analyze();
    }

    public void RemoveSyntaxTree(LuaDocumentId documentId)
    {
        _syntaxTrees.Remove(documentId);
        RemoveCache(documentId);
    }

    public void RemoveCache(LuaDocumentId documentId)
    {
        foreach (var luaAnalyzer in Analyzers)
        {
            luaAnalyzer.RemoveCache(documentId);
        }

        DeclarationTrees.Remove(documentId);
        Db.Remove(documentId);
        Diagnostics.RemoveCache(documentId);
    }

    public LuaSyntaxTree? GetSyntaxTree(LuaDocumentId documentId)
    {
        return _syntaxTrees.GetValueOrDefault(documentId);
    }

    public SemanticModel? GetSemanticModel(string url)
    {
        var document = Workspace.GetDocumentByUri(url);
        if (document is null)
        {
            return null;
        }

        var declarationTree = DeclarationTrees.GetValueOrDefault(document.Id);
        if (declarationTree is null)
        {
            return null;
        }

        return new SemanticModel(this, document, declarationTree);
    }

    public SemanticModel? GetSemanticModel(LuaDocumentId documentId)
    {
        var document = Workspace.GetDocument(documentId);
        if (document is null)
        {
            return null;
        }

        var declarationTree = DeclarationTrees.GetValueOrDefault(documentId);
        if (declarationTree is null)
        {
            return null;
        }

        return new SemanticModel(this, document, declarationTree);
    }

    private void Analyze()
    {
        if (DirtyDocumentIds.Count != 0)
        {
            try
            {
                var documents = new List<LuaDocument>();
                foreach (var documentId in DirtyDocumentIds)
                {
                    var document = Workspace.GetDocument(documentId);
                    if (document is not null && document.Text.Length < Workspace.Features.DontIndexMaxFileSize)
                    {
                        documents.Add(document);
                    }
                }

                var analyzeContext = new AnalyzeContext(documents);
                foreach (var analyzer in Analyzers)
                {
                    Workspace.Monitor?.OnAnalyzing(analyzer.Name);
                    analyzer.Analyze(analyzeContext);
                }

                foreach (var document in documents)
                {
                    Diagnostics.ClearDiagnostic(document.Id);
                    foreach (var diagnostic in document.SyntaxTree.Diagnostics)
                    {
                        Diagnostics.AddDiagnostic(document.Id, diagnostic);
                    }

                    document.SyntaxTree.Diagnostics.Clear();
                }
            }
            finally
            {
                DirtyDocumentIds.Clear();
            }
        }
    }

    private void AddDirtyDocument(LuaDocumentId documentId)
    {
        DirtyDocumentIds.Add(documentId);
    }

    public LuaDeclarationTree? GetDeclarationTree(LuaDocumentId documentId)
    {
        return DeclarationTrees.GetValueOrDefault(documentId);
    }

    public IEnumerable<Diagnostic> GetAllDiagnosticsParallel()
    {
        var result = new List<Diagnostic>();
        var context =
            new ThreadLocal<SearchContext>(() => new SearchContext(this, new SearchContextFeatures()));

        var diagnosticResults = Workspace.AllDocuments
            .AsParallel()
            .Select(it =>
            {
                // ReSharper disable once AccessToDisposedClosure
                if (Diagnostics.Check(it, context.Value!, out var documentDiagnostics))
                {
                    return documentDiagnostics;
                }

                return [];
            });
        context.Dispose();
        foreach (var diagnosticResult in diagnosticResults)
        {
            result.AddRange(diagnosticResult);
        }

        return result;
    }

    public IEnumerable<Diagnostic> GetAllDiagnostics()
    {
        var result = new List<Diagnostic>();
        var context = new SearchContext(this, new SearchContextFeatures());
        var diagnosticResults = Workspace.AllDocuments
            .Select(it =>
            {
                if (Diagnostics.Check(it, context, out var documentDiagnostics))
                {
                    return documentDiagnostics;
                }

                return [];
            });
        foreach (var diagnosticResult in diagnosticResults)
        {
            result.AddRange(diagnosticResult);
        }

        return result;
    }

    public IEnumerable<Diagnostic> GetDiagnostics(LuaDocumentId documentId, SearchContext context)
    {
        var document = Workspace.GetDocument(documentId);
        if (document is null)
        {
            return [];
        }

        return !Diagnostics.Check(document, context, out var results) ? [] : results;
    }
}
