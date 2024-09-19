﻿using EmmyLua.CodeAnalysis.Compilation.Search;
using EmmyLua.CodeAnalysis.Compilation.Symbol;
using EmmyLua.CodeAnalysis.Compilation.Type.TypeCompute;
using EmmyLua.CodeAnalysis.Compilation.Type.Types;
using EmmyLua.CodeAnalysis.Document;
using EmmyLua.CodeAnalysis.Syntax.Node;

namespace EmmyLua.CodeAnalysis.Compilation.Type.TypeInfo;

public abstract class LuaTypeInfo
{
    public abstract IEnumerable<LuaLocation> GetLocation(SearchContext context);

    public bool IsGeneric => GenericParameters != null;

    public abstract List<LuaSymbol>? GenericParameters { get; }

    public abstract LuaType? BaseType { get; }

    public abstract TypeComputer? TypeCompute { get; }

    public abstract List<LuaTypeRef>? Supers { get; }

    public abstract Dictionary<string, LuaSymbol>? Declarations { get; }

    public abstract Dictionary<string, LuaSymbol>? Implements { get; }

    public abstract Dictionary<TypeOperatorKind, List<TypeOperator>>? Operators { get; }

    public abstract NamedTypeKind Kind { get; }

    public abstract LuaTypeAttribute Attribute { get; }

    public bool Partial => Attribute.HasFlag(LuaTypeAttribute.Partial);

    public bool Exact => Attribute.HasFlag(LuaTypeAttribute.Exact);

    public bool Global => Attribute.HasFlag(LuaTypeAttribute.Global);

    public bool KeyEnum => Attribute.HasFlag(LuaTypeAttribute.Key);

    public abstract void AddDefineId(SyntaxElementId id);

    public abstract bool Remove(LuaDocumentId documentId, LuaTypeManager typeManager);

    public abstract bool IsDefinedInDocument(LuaDocumentId documentId);

    public abstract void AddDeclaration(LuaSymbol luaSymbol);

    public abstract void AddImplement(LuaSymbol luaSymbol);

    public abstract void AddSuper(LuaTypeRef super);

    public abstract void AddOperator(TypeOperatorKind kind, TypeOperator typeOperator);

    public abstract void AddGenericParameter(LuaSymbol genericParameter);

    public abstract void AddBaseType(LuaType baseType);
}