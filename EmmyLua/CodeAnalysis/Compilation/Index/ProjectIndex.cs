using EmmyLua.CodeAnalysis.Compilation.Declaration;
using EmmyLua.CodeAnalysis.Compilation.Reference;
using EmmyLua.CodeAnalysis.Compilation.Search;
using EmmyLua.CodeAnalysis.Compilation.Symbol;
using EmmyLua.CodeAnalysis.Compilation.Type.Types;
using EmmyLua.CodeAnalysis.Container;
using EmmyLua.CodeAnalysis.Document;
using EmmyLua.CodeAnalysis.Syntax.Node;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;


namespace EmmyLua.CodeAnalysis.Compilation.Index;

public class ProjectIndex
{
    private Dictionary<LuaDocumentId, LuaType> ModuleTypes { get; } = new();

    private Dictionary<LuaDocumentId, List<LuaElementPtr<LuaExprSyntax>>> ModuleReturns { get; } = new();

    private MultiIndex<string, LuaElementPtr<LuaNameExprSyntax>> NameExpr { get; } = new();

    private MultiIndex<string, LuaElementPtr<LuaIndexExprSyntax>> MultiIndexExpr { get; } = new();

    private MultiIndex<string, LuaElementPtr<LuaTableFieldSyntax>> TableField { get; } = new();

    private MultiIndex<string, LuaElementPtr<LuaDocNameTypeSyntax>> NameType { get; } = new();

    private InFileIndex<SyntaxElementId, List<LuaReference>> InFiledReferences { get; } = new();

    private InFileIndex<SyntaxElementId, LuaSymbol> InFiledDeclarations { get; } = new();

    private Dictionary<LuaDocumentId, LuaDeclarationTree> DocumentDeclarationTrees { get; } = new();

    private InFileIndex<SyntaxElementId, string> Source { get; } = new();

    private UniqueIndex<SyntaxElementId, string> MappingName { get; } = new();

    private GlobalIndex GlobalIndex { get; } = new();

    public void Remove(LuaDocumentId documentId)
    {
        ModuleTypes.Remove(documentId);
        ModuleReturns.Remove(documentId);
        NameExpr.Remove(documentId);
        MultiIndexExpr.Remove(documentId);
        NameType.Remove(documentId);
        InFiledReferences.Remove(documentId);
        InFiledDeclarations.Remove(documentId);
        DocumentDeclarationTrees.Remove(documentId);
        Source.Remove(documentId);
    }

    #region Add

    public void AddModuleReturns(LuaDocumentId documentId, LuaType type, List<LuaExprSyntax> exprs)
    {
        ModuleTypes[documentId] = type;
        ModuleReturns[documentId] = exprs.Select(it => it.ToPtr<LuaExprSyntax>()).ToList();
    }

    public void AddNameExpr(LuaDocumentId documentId, LuaNameExprSyntax nameExpr)
    {
        if (nameExpr.Name is { RepresentText: { } name })
        {
            NameExpr.Add(documentId, name, new(nameExpr));
        }
    }

    public void AddIndexExpr(LuaDocumentId documentId, LuaIndexExprSyntax indexExpr)
    {
        if (indexExpr is { Name: { } name })
        {
            MultiIndexExpr.Add(documentId, name, new(indexExpr));
        }
    }

    public void AddNameType(LuaDocumentId documentId, LuaDocNameTypeSyntax nameType)
    {
        if (nameType is { Name.RepresentText: { } name })
        {
            NameType.Add(documentId, name, new(nameType));
        }
    }

    public void AddReference(LuaDocumentId documentId, LuaSymbol symbol, LuaReference reference)
    {
        var list = InFiledReferences.Query(symbol.UniqueId);
        if (list is null)
        {
            list = [reference];
            InFiledReferences.Add(documentId, symbol.UniqueId, list);
        }
        else
        {
            list.Add(reference);
        }

        InFiledDeclarations.Add(documentId, reference.Ptr.UniqueId, symbol);
    }

    public void AddDeclarationTree(LuaDocumentId documentId, LuaDeclarationTree declarationTree)
    {
        DocumentDeclarationTrees[documentId] = declarationTree;
    }

    public void AddMapping(SyntaxElementId id, string name)
    {
        MappingName.Update(id.DocumentId, id, name);
    }

    public void AddTableField(LuaDocumentId documentId, LuaTableFieldSyntax tableField)
    {
        if (tableField.Name is { } name)
        {
            TableField.Add(documentId, name, new(tableField));
        }
    }

    public void AddSource(SyntaxElementId id, string source)
    {
        Source.Add(id.DocumentId, id, source);
    }

    public void AddGlobal(string name, LuaSymbol symbol)
    {
        GlobalIndex.AddGlobal(name, symbol);
    }

    #endregion

    #region Query

    public IEnumerable<LuaReference> QueryLocalReferences(LuaSymbol symbol)
    {
        var list = InFiledReferences.Query(symbol.UniqueId);
        if (list is not null)
        {
            return list;
        }

        return [];
    }

    public LuaSymbol? QueryLocalDeclaration(LuaSyntaxElement element)
    {
        return InFiledDeclarations.Query(element.UniqueId);
    }

    public IEnumerable<LuaSymbol> QueryDocumentLocalDeclarations(LuaDocumentId documentId)
    {
        var tree = DocumentDeclarationTrees.GetValueOrDefault(documentId);
        return tree is not null ? tree.Root.Descendants : [];
    }

    public LuaDeclarationTree? QueryDeclarationTree(LuaDocumentId documentId)
    {
        return DocumentDeclarationTrees.GetValueOrDefault(documentId);
    }

    public LuaType? QueryModuleType(LuaDocumentId documentId)
    {
        return ModuleTypes.GetValueOrDefault(documentId);
    }

    public IEnumerable<LuaIndexExprSyntax> QueryIndexExprReferences(string fieldName, SearchContext context)
    {
        foreach (var ptr in MultiIndexExpr.Query(fieldName))
        {
            if (ptr.ToNode(context) is { } node)
            {
                yield return node;
            }
        }
    }

    public IEnumerable<LuaTableFieldSyntax> QueryTableFieldReferences(string fieldName, SearchContext context)
    {
        foreach (var ptr in TableField.Query(fieldName))
        {
            if (ptr.ToNode(context) is { } node)
            {
                yield return node;
            }
        }
    }

    public IEnumerable<LuaNameExprSyntax> QueryNameExprReferences(string name, SearchContext context)
    {
        foreach (var ptr in NameExpr.Query(name))
        {
            if (ptr.ToNode(context) is { } node)
            {
                yield return node;
            }
        }
    }

    public IEnumerable<LuaElementPtr<LuaDocNameTypeSyntax>> QueryAllNamedType()
    {
        return NameType.QueryAll();
    }

    public IEnumerable<LuaElementPtr<LuaExprSyntax>> QueryModuleReturns(LuaDocumentId documentId)
    {
        return ModuleReturns.GetValueOrDefault(documentId) ?? [];
    }

    public string? QueryMapping(SyntaxElementId id)
    {
        return MappingName.Query(id);
    }

    public string? QuerySource(SyntaxElementId id)
    {
        return Source.Query(id);
    }

    public IEnumerable<(string, List<SyntaxElementId>)> QueryNamedElements(SearchContext context)
    {
        foreach (var pair in NameExpr.QueryAllWithKey())
        {
            var name = pair.Key;
            var elements = pair.Value.Select(it => it.Element.UniqueId).ToList();
            yield return (name, elements);
        }

        foreach (var pair in TableField.QueryAllWithKey())
        {
            var name = pair.Key;
            var elements = pair.Value.Select(it => it.Element.UniqueId).ToList();
            yield return (name, elements);
        }

        foreach (var pair in NameType.QueryAllWithKey())
        {
            var name = pair.Key;
            var elements = pair.Value.Select(it => it.Element.UniqueId).ToList();
            yield return (name, elements);
        }

        foreach (var pair in MultiIndexExpr.QueryAllWithKey())
        {
            var name = pair.Key;
            var elements = pair.Value.Select(it => it.Element.UniqueId).ToList();
            yield return (name, elements);
        }
    }

    #endregion
}
