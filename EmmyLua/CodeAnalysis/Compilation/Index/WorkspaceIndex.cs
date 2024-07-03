using EmmyLua.CodeAnalysis.Common;
using EmmyLua.CodeAnalysis.Compilation.Declaration;
using EmmyLua.CodeAnalysis.Compilation.Index.IndexContainer;
using EmmyLua.CodeAnalysis.Compilation.Reference;
using EmmyLua.CodeAnalysis.Compilation.Search;
using EmmyLua.CodeAnalysis.Compilation.Type;
using EmmyLua.CodeAnalysis.Document;
using EmmyLua.CodeAnalysis.Syntax.Node;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace EmmyLua.CodeAnalysis.Compilation.Index;

public class WorkspaceIndex
{
    private TypeIndex TypeIndex { get; } = new();

    private Dictionary<LuaDocumentId, LuaType> ModuleTypes { get; } = new();

    private Dictionary<LuaDocumentId, List<LuaElementPtr<LuaExprSyntax>>> ModuleReturns { get; } = new();

    private MultiIndex<string, LuaElementPtr<LuaNameExprSyntax>> NameExpr { get; } = new();

    private MultiIndex<string, LuaElementPtr<LuaIndexExprSyntax>> MultiIndexExpr { get; } = new();

    private MultiIndex<string, LuaElementPtr<LuaDocNameTypeSyntax>> NameType { get; } = new();

    private InFileIndex<SyntaxElementId, List<LuaReference>> InFiledReferences { get; } = new();

    private InFileIndex<SyntaxElementId, LuaDeclaration> InFiledDeclarations { get; } = new();

    private Dictionary<LuaDocumentId, LuaDeclarationTree> DocumentDeclarationTrees { get; } = new();

    public void Remove(LuaDocumentId documentId)
    {
        TypeIndex.Remove(documentId);
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

    public void AddMember(LuaDocumentId documentId, LuaType type, LuaDeclaration luaDeclaration)
    {
        if (!type.HasMember)
        {
            return;
        }

        switch (type)
        {
            case LuaNamedType namedType:
            {
                var name = namedType.Name;
                if (name == "global")
                {
                    TypeIndex.AddGlobal(documentId, false, luaDeclaration.Name, luaDeclaration);
                    return;
                }

                TypeIndex.AddNamedTypeMember(documentId, namedType, luaDeclaration);
                break;
            }
            case GlobalNameType globalNameType:
            {
                var name = globalNameType.Name;
                TypeIndex.AddGlobalVariableMember(documentId, name, luaDeclaration);
                break;
            }
            case LuaVariableRefType variableRefType:
            {
                TypeIndex.AddLocalVariableMember(variableRefType.Id, luaDeclaration);
                break;
            }
            case LuaDocTableType docTableType:
            {
                TypeIndex.AddLocalVariableMember(docTableType.DocTablePtr.UniqueId, luaDeclaration);
                break;
            }
            case LuaTableLiteralType luaTableLiteral:
            {
                TypeIndex.AddLocalVariableMember(luaTableLiteral.TableExprPtr.UniqueId, luaDeclaration);
                break;
            }
        }
    }

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

    public void AddTypeOperator(LuaDocumentId documentId, TypeOperator typeOperator)
    {
        TypeIndex.AddTypeOperator(documentId, typeOperator);
    }

    public void AddTypeOverload(LuaDocumentId documentId, string name, LuaMethodType methodType)
    {
        TypeIndex.AddTypeOverload(documentId, name, methodType);
    }

    public void AddReference(LuaDocumentId documentId, LuaDeclaration declaration, LuaReference reference)
    {
        var list = InFiledReferences.Query(documentId, declaration.UniqueId);
        if (list is null)
        {
            list = [reference];
            InFiledReferences.Add(documentId, declaration.UniqueId, list);
        }
        else
        {
            list.Add(reference);
        }

        InFiledDeclarations.Add(documentId, reference.Ptr.UniqueId, declaration);
    }

    public void AddDeclarationTree(LuaDocumentId documentId, LuaDeclarationTree declarationTree)
    {
        DocumentDeclarationTrees[documentId] = declarationTree;
    }

    public void UpdateIdRelatedType(SyntaxElementId id, LuaType type)
    {
        TypeIndex.UpdateIdRelatedType(id, type);
    }

    public void AddGlobalRelationType(LuaDocumentId documentId, string name, LuaType type)
    {
        TypeIndex.AddGlobalRelationType(documentId, name, type);
    }

    public void AddSuper(LuaDocumentId documentId, string name, LuaType super)
    {
        TypeIndex.AddSuper(documentId, name, super);
    }

    public void AddGlobal(LuaDocumentId documentId, bool forceDefine, string name, LuaDeclaration declaration)
    {
        TypeIndex.AddGlobal(documentId, forceDefine, name, declaration);
    }

    public void AddEnum(LuaDocumentId documentId, string name, LuaType? baseType, LuaDeclaration declaration)
    {
        TypeIndex.AddEnum(documentId, name, baseType, declaration);
    }

    public void AddTypeDefinition(LuaDocumentId documentId, string name, LuaDeclaration declaration)
    {
        TypeIndex.AddTypeDefinition(documentId, name, declaration);
    }

    public void AddAlias(LuaDocumentId documentId, string name, LuaType baseType, LuaDeclaration declaration)
    {
        TypeIndex.AddAlias(documentId, name, baseType, declaration);
    }

    public void AddGenericParam(LuaDocumentId documentId, string name, LuaDeclaration luaDeclaration)
    {
        TypeIndex.AddGenericParam(documentId, name, luaDeclaration);
    }

    #endregion

    #region Query

    public IEnumerable<IDeclaration> QueryAllGlobal()
    {
        return TypeIndex.QueryAllGlobal();
    }

    public IEnumerable<IDeclaration> QueryMembers(LuaType type)
    {
        return TypeIndex.QueryMembers(type);
    }

    public IDeclaration? QueryGlobals(string name)
    {
        return TypeIndex.QueryGlobals(name);
    }

    public IEnumerable<LuaType> QuerySupers(string name)
    {
        return TypeIndex.QuerySupers(name);
    }

    public IEnumerable<string> QuerySubTypes(string name)
    {
        return TypeIndex.QuerySubTypes(name);
    }

    public IEnumerable<IDeclaration> QueryNamedTypeDefinitions(string name)
    {
        return TypeIndex.QueryNamedTypeDefinitions(name);
    }

    public LuaType? QueryAliasOriginTypes(string name)
    {
        return TypeIndex.QueryAliasOriginTypes(name);
    }

    public IEnumerable<IDeclaration> QueryGenericParams(string name)
    {
        return TypeIndex.QueryGenericParams(name);
    }

    public NamedTypeKind QueryNamedTypeKind(string name)
    {
        var typeDeclaration = QueryNamedTypeDefinitions(name).OfType<LuaDeclaration>().FirstOrDefault();
        if (typeDeclaration is { Info: NamedTypeInfo { Kind: { } kind } })
        {
            return kind;
        }

        return NamedTypeKind.None;
    }

    public IEnumerable<IDeclaration> QueryAllNamedTypeDefinitions()
    {
        return TypeIndex.QueryAllNamedTypeDefinitions();
    }

    public IEnumerable<TypeOperator> QueryTypeOperators(string name)
    {
        return TypeIndex.QueryTypeOperators(name);
    }

    public IEnumerable<LuaMethodType> QueryTypeOverloads(string name)
    {
        return TypeIndex.QueryTypeOverloads(name);
    }

    public IEnumerable<LuaReference> QueryLocalReferences(LuaDeclaration declaration)
    {
        var list = InFiledReferences.Query(declaration.Info.Ptr.DocumentId, declaration.UniqueId);
        if (list is not null)
        {
            return list;
        }

        return [];
    }

    public LuaDeclaration? QueryLocalDeclaration(LuaSyntaxElement element)
    {
        return InFiledDeclarations.Query(element.DocumentId, element.UniqueId);
    }

    public IEnumerable<LuaDeclaration> QueryDocumentLocalDeclarations(LuaDocumentId documentId)
    {
        var tree = DocumentDeclarationTrees.GetValueOrDefault(documentId);
        return tree is not null ? tree.Root.Descendants : [];
    }

    public LuaDeclarationTree? QueryDeclarationTree(LuaDocumentId documentId)
    {
        return DocumentDeclarationTrees.GetValueOrDefault(documentId);
    }

    public LuaType? QueryRelatedGlobalType(string name)
    {
        return TypeIndex.QueryRelatedGlobalType(name);
    }

    public LuaType? QueryModuleType(LuaDocumentId documentId)
    {
        return ModuleTypes.GetValueOrDefault(documentId);
    }

    public LuaType? QueryTypeFromId(SyntaxElementId id)
    {
        return TypeIndex.QueryTypeFromId(id);
    }

    public LuaType? QueryParentType(LuaSyntaxNode node)
    {
        return TypeIndex.QueryParentType(node.UniqueId);
    }

    public LuaType? QueryParentType(SyntaxElementId id)
    {
        return TypeIndex.QueryParentType(id);
    }

    public IEnumerable<LuaIndexExprSyntax> QueryIndexExprReferences(string fieldName, SearchContext context)
    {
        foreach (var ptr in MultiIndexExpr.Query(fieldName))
        {
            if (ptr.ToNode(context) is {} node)
            {
                yield return node;
            }
        }
    }

    public IEnumerable<LuaNameExprSyntax> QueryNameExprReferences(string name, SearchContext context)
    {
        foreach (var ptr in NameExpr.Query(name))
        {
            if (ptr.ToNode(context) is {} node)
            {
                yield return node;
            }
        }
    }

    public IEnumerable<LuaDocNameTypeSyntax> QueryNamedTypeReferences(string name, SearchContext context)
    {
        foreach (var ptr in NameType.Query(name))
        {
            if (ptr.ToNode(context) is {} node)
            {
                yield return node;
            }
        }
    }

    public IEnumerable<LuaElementPtr<LuaExprSyntax>> QueryModuleReturns(LuaDocumentId documentId)
    {
        return ModuleReturns.GetValueOrDefault(documentId) ?? [];
    }

    public IEnumerable<LuaDeclaration> QueryAllMembers()
    {
        return TypeIndex.QueryAllMembers();
    }
    #endregion
}
