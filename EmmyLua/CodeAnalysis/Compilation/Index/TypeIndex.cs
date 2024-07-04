using EmmyLua.CodeAnalysis.Common;
using EmmyLua.CodeAnalysis.Compilation.Declaration;
using EmmyLua.CodeAnalysis.Compilation.Index.IndexContainer;
using EmmyLua.CodeAnalysis.Compilation.Type;
using EmmyLua.CodeAnalysis.Document;
using EmmyLua.CodeAnalysis.Syntax.Node;

namespace EmmyLua.CodeAnalysis.Compilation.Index;

public class TypeIndex
{
    #region Members

    private MultiIndex<string, LuaDeclaration> TypeMembers { get; } = new();

    private MultiIndex<SyntaxElementId, LuaDeclaration> DocumentVariableMembers { get; } = new();

    private MultiIndex<string, LuaDeclaration> GlobalVariableMembers { get; } = new();

    #endregion

    #region ParentType

    private UniqueIndex<SyntaxElementId, string> ParentNamedType { get; } = new();

    private UniqueIndex<SyntaxElementId, SyntaxElementId> ParentVariableType { get; } = new();

    private UniqueIndex<SyntaxElementId, string> ParentGlobalType { get; } = new();

    #endregion

    #region AssociatedType

    private UniqueIndex<SyntaxElementId, LuaType> AssociatedType { get; } = new();

    private UniqueIndex<string, LuaType> GlobalAssociatedType { get; } = new();

    #endregion

    #region TypeInfo
    private MultiIndex<string, LuaType> Supers { get; } = new();

    private MultiIndex<string, string> SubTypes { get; } = new();

    private MultiIndex<string, LuaDeclaration> NamedTypeDefinition { get; } = new();

    private UniqueIndex<string, LuaType> AliasTypes { get; } = new();

    private MultiIndex<string, LuaDeclaration> GenericParams { get; } = new();

    private MultiIndex<string, TypeOperator> TypeOperator { get; } = new();

    private MultiIndex<string, LuaMethodType> TypeOverloads { get; } = new();


    #endregion

    public void Remove(LuaDocumentId documentId)
    {
        // members
        TypeMembers.Remove(documentId);
        DocumentVariableMembers.Remove(documentId);
        GlobalVariableMembers.Remove(documentId);
        // parents
        ParentNamedType.Remove(documentId);
        ParentVariableType.Remove(documentId);
        ParentGlobalType.Remove(documentId);

        // AssociatedType
        AssociatedType.Remove(documentId);
        GlobalAssociatedType.Remove(documentId);

        // typeInfo
        Supers.Remove(documentId);
        SubTypes.Remove(documentId);
        NamedTypeDefinition.Remove(documentId);
        GenericParams.Remove(documentId);
        AliasTypes.Remove(documentId);
        TypeOverloads.Remove(documentId);
        TypeOperator.Remove(documentId);
    }

    public void AddNamedTypeMember(LuaDocumentId documentId, LuaNamedType namedType, LuaDeclaration luaDeclaration)
    {
        TypeMembers.Add(documentId, namedType.Name, luaDeclaration);
        ParentNamedType.Update(documentId, luaDeclaration.UniqueId, namedType.Name);
    }

    public void AddLocalVariableMember(SyntaxElementId id, LuaDeclaration luaDeclaration)
    {
        DocumentVariableMembers.Add(id.DocumentId, id, luaDeclaration);
        ParentVariableType.Update(id.DocumentId, luaDeclaration.UniqueId, id);
    }

    public void AddGlobalVariableMember(LuaDocumentId documentId, string name, LuaDeclaration luaDeclaration)
    {
        GlobalVariableMembers.Add(documentId, name, luaDeclaration);
        ParentGlobalType.Update(documentId, luaDeclaration.UniqueId, name);
    }

    public void AddParentNamedType(LuaDocumentId documentId, string name, LuaDeclaration luaDeclaration)
    {
        ParentNamedType.Update(documentId, luaDeclaration.UniqueId, name);
    }

    public void AddSuper(LuaDocumentId documentId, string name, LuaType type)
    {
        if (type is LuaNamedType { Name: { } name1 } && string.Equals(name, name1, StringComparison.CurrentCulture))
        {
            return;
        }

        Supers.Add(documentId, name, type);
        if (type is LuaNamedType namedType)
        {
            SubTypes.Add(documentId, namedType.Name, name);
        }
    }

    public void AddTypeDefinition(LuaDocumentId documentId, string name, LuaDeclaration declaration)
    {
        NamedTypeDefinition.Add(documentId, name, declaration);
    }

    public void AddAlias(LuaDocumentId documentId, string name, LuaType baseType,
        LuaDeclaration declaration)
    {
        AddTypeDefinition(documentId, name, declaration);
        AliasTypes.Update(documentId, name, baseType);
    }

    public void UpdateIdRelatedType(SyntaxElementId id, LuaType relatedType)
    {
        AssociatedType.Update(id.DocumentId, id, relatedType);
    }

    public void AddGlobalRelationType(LuaDocumentId documentId, string name, LuaType type)
    {
        if (type.Equals(Builtin.Unknown))
        {
            return;
        }

        if (GlobalAssociatedType.Query(name) is { } oldType)
        {
            GlobalAssociatedType.Update(documentId, name, oldType.Union(type));
        }
        else
        {
            GlobalAssociatedType.Update(documentId, name, type);
        }
    }

    public void AddEnum(LuaDocumentId documentId, string name, LuaType? baseType,
        LuaDeclaration declaration)
    {
        AddTypeDefinition(documentId, name, declaration);
        if (baseType != null)
        {
            AddSuper(documentId, name, baseType);
        }
    }

    public void AddGenericParam(LuaDocumentId documentId, string name, LuaDeclaration luaDeclaration)
    {
        GenericParams.Add(documentId, name, luaDeclaration);
    }

    public void AddTypeOperator(LuaDocumentId documentId, TypeOperator typeOperator)
    {
        TypeOperator.Add(documentId, typeOperator.BelongTypeName, typeOperator);
    }

    public void AddTypeOverload(LuaDocumentId documentId, string name, LuaMethodType methodType)
    {
        TypeOverloads.Add(documentId, name, methodType);
    }

    public IEnumerable<IDeclaration> QueryMembers(LuaType type)
    {
        if (!type.HasMember)
        {
            return [];
        }

        switch (type)
        {
            case LuaNamedType namedType:
            {
                var name = namedType.Name;
                return TypeMembers.Query(name);
            }
            case GlobalNameType globalNameType:
            {
                var name = globalNameType.Name;
                return GlobalVariableMembers.Query(name);
            }
            case LuaVariableRefType variableRefType:
            {
                return DocumentVariableMembers.Query(variableRefType.Id);
            }
            case LuaDocTableType docTableType:
            {
                return DocumentVariableMembers.Query(docTableType.DocTablePtr.UniqueId);
            }
            case LuaTableLiteralType luaTableLiteral:
            {
                return DocumentVariableMembers.Query(luaTableLiteral.TableExprPtr.UniqueId);
            }
        }

        return [];
    }

    public IEnumerable<LuaType> QuerySupers(string name)
    {
        return Supers.Query(name);
    }

    public IEnumerable<string> QuerySubTypes(string name)
    {
        return SubTypes.Query(name);
    }

    public IEnumerable<IDeclaration> QueryNamedTypeDefinitions(string name)
    {
        return NamedTypeDefinition.Query(name);
    }

    public LuaType? QueryAliasOriginTypes(string name)
    {
        return AliasTypes.Query(name);
    }

    public IEnumerable<IDeclaration> QueryGenericParams(string name)
    {
        return GenericParams.Query(name);
    }

    public IEnumerable<IDeclaration> QueryAllNamedTypeDefinitions()
    {
        return NamedTypeDefinition.QueryAll();
    }

    public IEnumerable<TypeOperator> QueryTypeOperators(string name)
    {
        return TypeOperator.Query(name);
    }

    public IEnumerable<LuaMethodType> QueryTypeOverloads(string name)
    {
        return TypeOverloads.Query(name);
    }

    public LuaType? QueryRelatedGlobalType(string name)
    {
        return GlobalAssociatedType.Query(name);
    }

    public LuaType? QueryTypeFromId(SyntaxElementId id)
    {
        return AssociatedType.Query(id);
    }

    public LuaType? QueryParentType(SyntaxElementId id)
    {
        if (ParentNamedType.Query(id) is { } parentName)
        {
            return new LuaNamedType(parentName);
        }

        if (ParentGlobalType.Query(id) is { } parentGlobal)
        {
            return new GlobalNameType(parentGlobal);
        }

        if (ParentVariableType.Query(id) is {} parentId)
        {
            return new LuaVariableRefType(parentId);
        }

        return null;
    }

    public IEnumerable<LuaDeclaration> QueryAllMembers()
    {
        return TypeMembers.QueryAll();
    }
}
