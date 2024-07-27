using EmmyLua.CodeAnalysis.Compilation.Declaration;
using EmmyLua.CodeAnalysis.Compilation.Reference;
using EmmyLua.CodeAnalysis.Compilation.Search;
using EmmyLua.CodeAnalysis.Compilation.Symbol;
using EmmyLua.CodeAnalysis.Container;
using EmmyLua.CodeAnalysis.Document;
using EmmyLua.CodeAnalysis.Syntax.Node;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;
using EmmyLua.CodeAnalysis.Type;


namespace EmmyLua.CodeAnalysis.Compilation.Index;

public class ProjectIndex
{
    private Dictionary<LuaDocumentId, LuaType> ModuleTypes { get; } = new();

    private Dictionary<LuaDocumentId, List<LuaElementPtr<LuaExprSyntax>>> ModuleReturns { get; } = new();

    private MultiIndex<string, LuaElementPtr<LuaNameExprSyntax>> NameExpr { get; } = new();

    private MultiIndex<string, LuaElementPtr<LuaIndexExprSyntax>> MultiIndexExpr { get; } = new();

    private MultiIndex<string, LuaElementPtr<LuaDocNameTypeSyntax>> NameType { get; } = new();

    private InFileIndex<SyntaxElementId, List<LuaReference>> InFiledReferences { get; } = new();

    private InFileIndex<SyntaxElementId, LuaSymbol> InFiledDeclarations { get; } = new();

    private Dictionary<LuaDocumentId, LuaDeclarationTree> DocumentDeclarationTrees { get; } = new();

    private UniqueIndex<SyntaxElementId, string> MappingName { get; } = new();

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

    public IEnumerable<LuaDocNameTypeSyntax> QueryNamedTypeReferences(string name, SearchContext context)
    {
        foreach (var ptr in NameType.Query(name))
        {
            if (ptr.ToNode(context) is { } node)
            {
                yield return node;
            }
        }
    }

    public IEnumerable<LuaElementPtr<LuaExprSyntax>> QueryModuleReturns(LuaDocumentId documentId)
    {
        return ModuleReturns.GetValueOrDefault(documentId) ?? [];
    }

    public string? QueryMapping(SyntaxElementId id)
    {
        return MappingName.Query(id);
    }

    #endregion
}
