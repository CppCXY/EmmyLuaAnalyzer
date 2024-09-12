﻿using EmmyLua.CodeAnalysis.Compilation.Search;
using EmmyLua.CodeAnalysis.Compilation.Symbol;
using EmmyLua.CodeAnalysis.Compilation.Type.TypeCompute;
using EmmyLua.CodeAnalysis.Compilation.Type.Types;
using EmmyLua.CodeAnalysis.Document;
using EmmyLua.CodeAnalysis.Syntax.Node;

namespace EmmyLua.CodeAnalysis.Compilation.Type.TypeInfo;

public class LuaComputerTypeInfo(SyntaxElementId id, TypeComputer typeComputer) : LuaTypeInfo
{
    private SyntaxElementId _elementId = id;

    public override IEnumerable<LuaLocation> GetLocation(SearchContext context)
    {
        if (_elementId.GetLocation(context) is { } location)
        {
            yield return location;
        }
    }

    public override List<LuaSymbol>? GenericParameters => null;

    public override LuaType? BaseType => null;

    public override TypeComputer TypeCompute => typeComputer;

    public override List<LuaNamedType>? Supers => null;

    public override Dictionary<string, LuaSymbol>? Declarations => null;

    public override Dictionary<string, LuaSymbol>? Implements => null;

    public override Dictionary<TypeOperatorKind, List<TypeOperator>>? Operators => null;

    public override NamedTypeKind Kind => NamedTypeKind.TypeCompute;

    public override LuaTypeAttribute Attribute => LuaTypeAttribute.None;

    public override void AddDefineId(SyntaxElementId id)
    {
    }

    public override bool Remove(LuaDocumentId documentId, LuaTypeManager typeManager)
    {
        if (IsDefinedInDocument(documentId))
        {
            return true;
        }

        return false;
    }

    public override bool IsDefinedInDocument(LuaDocumentId documentId)
    {
        return _elementId.DocumentId == documentId;
    }

    public override void AddDeclaration(LuaSymbol luaSymbol)
    {
    }

    public override void AddImplement(LuaSymbol luaSymbol)
    {
    }

    public override void AddSuper(LuaNamedType super)
    {
    }

    public override void AddOperator(TypeOperatorKind kind, TypeOperator typeOperator)
    {
    }

    public override void AddGenericParameter(LuaSymbol genericParameter)
    {
    }

    public override void AddBaseType(LuaType baseType)
    {
    }
}
