using EmmyLua.CodeAnalysis.Compilation.Declaration;
using EmmyLua.CodeAnalysis.Compilation.Scope;
using EmmyLua.CodeAnalysis.Compilation.Search;
using EmmyLua.CodeAnalysis.Compilation.Symbol;
using EmmyLua.CodeAnalysis.Diagnostics;
using EmmyLua.CodeAnalysis.Document;
using EmmyLua.CodeAnalysis.Syntax.Node;
using EmmyLua.CodeAnalysis.Type;


namespace EmmyLua.CodeAnalysis.Compilation.Semantic;

public class SemanticModel(LuaCompilation compilation, LuaDocument document)
{
    public LuaCompilation Compilation { get; } = compilation;

    public LuaDocument Document { get; } = document;

    public SearchContext Context { get; } = new(compilation, new SearchContextFeatures());

    public IEnumerable<ReferenceResult> FindReferences(LuaSyntaxElement element)
    {
        var declaration = Context.FindDeclaration(element);
        if (declaration is not null)
        {
            return Context.FindReferences(declaration);
        }

        return [];
    }

    public IEnumerable<ReferenceResult> FindImplementations(LuaSyntaxElement element)
    {
        // var symbol = Context.FindDeclaration(element);
        // if (symbol is not null)
        // {
        //     return Context.FindImplementations(symbol);
        // }

        return [];
    }

    public IEnumerable<Diagnostic> GetDiagnostics()
    {
        return Compilation.GetDiagnostics(Document.Id, Context);
    }

    public IEnumerable<LuaSymbol> GetGlobals()
    {
        throw new NotImplementedException();
        // return Compilation.TypeManager.
    }

    public IEnumerable<LuaSymbol> GetDeclarationsBefore(LuaSyntaxElement beforeToken)
    {
        var result = new List<LuaSymbol>();
        var token = Document.SyntaxTree.SyntaxRoot.TokenAt(beforeToken.Position);
        if (Compilation.Db.QueryDeclarationTree(beforeToken.DocumentId) is { } tree && token is not null)
        {
            var scope = tree.FindScope(token);
            scope?.WalkUp(beforeToken.Position, 0, declaration =>
            {
                result.Add(declaration);
                return ScopeFoundState.NotFounded;
            });
            return result;
        }

        return [];
    }

    public LuaType? GetExportType(LuaDocumentId documentId)
    {
        return Compilation.Db.QueryModuleType(documentId);
    }
}
