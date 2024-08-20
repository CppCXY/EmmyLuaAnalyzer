﻿using EmmyLua.CodeAnalysis.Compilation.Symbol;
using EmmyLua.CodeAnalysis.Document;
using EmmyLua.CodeAnalysis.Syntax.Node;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;
using EmmyLua.CodeAnalysis.Type;
using EmmyLua.CodeAnalysis.Type.Manager.TypeInfo;
using EmmyLua.CodeAnalysis.Type.Types;

namespace EmmyLua.CodeAnalysis.Compilation.Search;

public class IndexMembers(SearchContext context)
{
    private HashSet<ITypeInfo> _visitedTypes = new();

    public LuaSymbol? FindTypeMember(LuaType type, string name)
    {
        return type switch
        {
            LuaGenericType genericType => FindGenericTypeMember(genericType, name),
            LuaNamedType namedType => FindNamedTypeMember(namedType, name),
            LuaElementType elementType => FindElementTypeMember(elementType, name),
            GlobalNameType globalType => FindGlobalTypeMember(globalType, name),
            LuaTupleType tupleType => FindTupleTypeMember(tupleType, name),
            LuaUnionType unionType => FindUnionTypeMember(unionType, name),
            LuaArrayType arrayType => FindArrayTypeMember(arrayType, name),
            _ => null
        };
    }

    private LuaSymbol? FindNamedTypeMember(LuaNamedType namedType, string name)
    {
        if (namedType.IsSameType(Builtin.Global, context))
        {
            return context.Compilation.TypeManager.GetGlobalSymbol(name);
        }

        var typeInfo = context.Compilation.TypeManager.FindTypeInfo(namedType);
        if (typeInfo is null)
        {
            return null;
        }

        if (!_visitedTypes.Add(typeInfo))
        {
            return null;
        }

        try
        {
            switch (typeInfo.Kind)
            {
                case NamedTypeKind.Class:
                {
                    if (typeInfo.Declarations is { } declarations)
                    {
                        if (declarations.TryGetValue(name, out var symbol))
                        {
                            return symbol;
                        }
                    }

                    break;
                }
                case NamedTypeKind.Interface:
                {
                    if (typeInfo.Declarations is { } declarations)
                    {
                        if (declarations.TryGetValue(name, out var symbol))
                        {
                            return symbol;
                        }
                    }

                    break;
                }
                case NamedTypeKind.Enum:
                {
                    if (typeInfo.Declarations is { } declarations)
                    {
                        if (declarations.TryGetValue(name, out var symbol))
                        {
                            var enumMemberType = new EnumInstanceType(namedType, typeInfo.BaseType ?? symbol.Type);
                            return symbol.WithType(enumMemberType);
                        }
                    }

                    return null;
                }
                case NamedTypeKind.Alias:
                {
                    if (typeInfo.BaseType is not null)
                    {
                        return FindTypeMember(typeInfo.BaseType, name);
                    }

                    return null;
                }
            }

            if (typeInfo.BaseType is not null)
            {
                return FindTypeMember(typeInfo.BaseType, name);
            }

            if (typeInfo.Supers is { } supers)
            {
                foreach (var super in supers)
                {
                    if (FindTypeMember(super, name) is { } symbol)
                    {
                        return symbol;
                    }
                }
            }

            if (name.StartsWith('['))
            {
                var op = context.GetBestMatchedIndexOperator(namedType, Builtin.Integer);
                if (op is not null)
                {
                    return new LuaSymbol(name, op.Ret, new VirtualInfo());
                }
            }
            else
            {
                var op = context.GetBestMatchedIndexOperator(namedType, Builtin.String);
                if (op is not null)
                {
                    return new LuaSymbol(name, op.Ret, new VirtualInfo());
                }
            }

            return null;
        }
        finally
        {
            _visitedTypes.Remove(typeInfo);
        }
    }

    private LuaSymbol? FindElementTypeMember(LuaElementType elementType, string name)
    {
        var typeInfo = context.Compilation.TypeManager.FindTypeInfo(elementType.Id);
        if (typeInfo is null)
        {
            return null;
        }

        if (!_visitedTypes.Add(typeInfo))
        {
            return null;
        }

        try
        {
            if (typeInfo.Declarations is { } declarations)
            {
                if (declarations.TryGetValue(name, out var symbol))
                {
                    return symbol;
                }
            }

            if (typeInfo.BaseType is not null)
            {
                return FindTypeMember(typeInfo.BaseType, name);
            }

            return null;
        }
        finally
        {
            _visitedTypes.Remove(typeInfo);
        }
    }

    private LuaSymbol? FindGlobalTypeMember(GlobalNameType globalType, string name)
    {
        var namedType = context.Compilation.TypeManager.GetGlobalProxyNameType(globalType.Name);
        if (namedType is not null)
        {
            return FindNamedTypeMember(namedType, name);
        }

        var typeInfo = context.Compilation.TypeManager.FindGlobalInfo(globalType.Name);
        if (typeInfo is null)
        {
            return null;
        }

        if (!_visitedTypes.Add(typeInfo))
        {
            return null;
        }

        try
        {
            if (typeInfo.Declarations is { } declarations)
            {
                if (declarations.TryGetValue(name, out var symbol))
                {
                    return symbol;
                }
            }

            if (typeInfo.BaseType is not null)
            {
                return FindTypeMember(typeInfo.BaseType, name);
            }

            return null;
        }
        finally
        {
            _visitedTypes.Remove(typeInfo);
        }
    }

    private LuaSymbol? FindTupleTypeMember(LuaTupleType tupleType, string name)
    {
        if (name.StartsWith('[') && name.EndsWith(']'))
        {
            if (int.TryParse(name[1..^1], out var result))
            {
                return VirtualInfo.CreateTypeSymbol(tupleType.TypeList.ElementAtOrDefault(result - 1));
            }
        }

        return null;
    }

    private LuaSymbol? FindUnionTypeMember(LuaUnionType unionType, string name)
    {
        foreach (var type in unionType.UnionTypes)
        {
            if (FindTypeMember(type, name) is { } symbol)
            {
                return symbol;
            }
        }

        return null;
    }

    private LuaSymbol? FindGenericTypeMember(LuaGenericType genericType, string name)
    {
        if (genericType.Name == "table")
        {
            if (name.StartsWith('[') && genericType.GenericArgs.FirstOrDefault() is { } numberType
                                     && (numberType.IsSameType(Builtin.Number, context) ||
                                         numberType.IsSameType(Builtin.Integer, context)))
            {
                return VirtualInfo.CreateTypeSymbol(genericType.GenericArgs.ElementAtOrDefault(1));
            }
            else if (genericType.GenericArgs.FirstOrDefault() is { } stringType &&
                     stringType.IsSameType(Builtin.String, context))
            {
                return VirtualInfo.CreateTypeSymbol(genericType.GenericArgs.ElementAtOrDefault(1));
            }
        }
        else if (genericType.Name == "namespace" &&
                 genericType.GenericArgs.FirstOrDefault() is LuaStringLiteralType namespaceString)
        {
            var namespaceOrTypeInfo =
                context.Compilation.TypeManager.FindNamespaceOrType(namespaceString.Content, name);
            if (!namespaceOrTypeInfo.HasValue)
            {
                return null;
            }

            if (namespaceOrTypeInfo.Value.IsNamespace)
            {
                var namespaceType = new LuaGenericType(LuaDocumentId.VirtualDocumentId, "namespace", [
                    new LuaStringLiteralType($"{namespaceString.Content}.{name}")
                ]);
                return new LuaSymbol(name,
                    namespaceType,
                    new NamespaceInfo()
                );
            }
            else
            {
                var namedType = new LuaNamedType(LuaDocumentId.VirtualDocumentId,
                    $"{namespaceString.Content}.{name}");
                return new LuaSymbol(name, namedType, new NamedTypeInfo(
                    new LuaElementPtr<LuaDocTagNamedTypeSyntax>(namespaceOrTypeInfo.Value.Id),
                    namespaceOrTypeInfo.Value.Kind
                ));
            }
        }

        var typeInfo = context.Compilation.TypeManager.FindTypeInfo(genericType);
        if (typeInfo is null)
        {
            return null;
        }

        if (!_visitedTypes.Add(typeInfo))
        {
            return null;
        }

        try
        {
            var substitute = new TypeSubstitution();
            var genericArgs = genericType.GenericArgs;
            if (typeInfo.GenericParams is not null)
            {
                for (var i = 0; i < typeInfo.GenericParams.Count && i < genericArgs.Count; i++)
                {
                    substitute.Add(typeInfo.GenericParams[i].Name, genericArgs[i], true);
                }
            }

            if (typeInfo.Kind is NamedTypeKind.Class or NamedTypeKind.Interface)
            {
                if (typeInfo.Declarations is { } declarations)
                {
                    if (declarations.TryGetValue(name, out var symbol))
                    {
                        return symbol.Instantiate(substitute);
                    }
                }

                if (typeInfo.BaseType is not null)
                {
                    return FindTypeMember(typeInfo.BaseType, name)?.Instantiate(substitute);
                }

                if (typeInfo.Supers is { } supers)
                {
                    foreach (var super in supers)
                    {
                        if (FindTypeMember(super, name) is { } symbol)
                        {
                            return symbol;
                        }
                    }
                }
            }

            return null;
        }
        finally
        {
            _visitedTypes.Remove(typeInfo);
        }
    }

    private LuaSymbol? FindArrayTypeMember(LuaArrayType arrayType, string name)
    {
        if (name.StartsWith('['))
        {
            return new LuaSymbol(string.Empty, arrayType.BaseType, new VirtualInfo());
        }

        return null;
    }

    public LuaSymbol? FindTypeMember(LuaType type, LuaIndexExprSyntax indexExpr)
    {
        if (indexExpr.Name is { } name)
        {
            return FindTypeMember(type, name);
        }
        else if (type is LuaArrayType arrayType)
        {
            return new LuaSymbol(string.Empty, arrayType.BaseType, new VirtualInfo());
        }
        else if (type is LuaGenericType genericType)
        {
            return FindGenericTypeMember(genericType, indexExpr);
        }
        else if (type is LuaNamedType namedType && indexExpr.IndexKeyExpr is { } indexKeyExpr)
        {
            var keyType = context.Infer(indexKeyExpr);
            if (keyType.IsSameType(Builtin.Unknown, context))
            {
                return null;
            }

            if (keyType is LuaNamedType keyNamedType)
            {
                var op = context.GetBestMatchedIndexOperator(namedType, keyNamedType);
                if (op is not null)
                {
                    return new LuaSymbol(string.Empty, op.Ret, new VirtualInfo());
                }
            }
            else if (keyType is EnumInstanceType { EnumType: { } enumType })
            {
                var op = context.GetBestMatchedIndexOperator(namedType, enumType);
                if (op is not null)
                {
                    return new LuaSymbol(string.Empty, op.Ret, new VirtualInfo());
                }
            }

            return null;
        }

        return null;
    }

    public LuaSymbol? FindSuperMember(LuaType luaType, string name)
    {
        if (luaType is LuaNamedType namedType)
        {
            var typeInfo = context.Compilation.TypeManager.FindTypeInfo(namedType);
            if (typeInfo is null)
            {
                return null;
            }

            if (!_visitedTypes.Add(typeInfo))
            {
                return null;
            }

            try
            {
                if (typeInfo.Kind is NamedTypeKind.Class or NamedTypeKind.Interface)
                {
                    if (typeInfo.BaseType is { } baseType)
                    {
                        if (FindTypeMember(baseType, name) is { } symbol)
                        {
                            return symbol;
                        }
                    }

                    if (typeInfo.Supers is { } supers)
                    {
                        foreach (var super in supers)
                        {
                            if (FindTypeMember(super, name) is { } symbol)
                            {
                                return symbol;
                            }
                        }
                    }
                }

                return null;
            }
            finally
            {
                _visitedTypes.Remove(typeInfo);
            }
        }

        return null;
    }

    private LuaSymbol? FindGenericTypeMember(LuaGenericType type, LuaIndexExprSyntax indexExpr)
    {
        if (type.Name == "table")
        {
            if (type.GenericArgs.Count != 2)
            {
                return null;
            }

            if (indexExpr.IndexKeyExpr is { } indexKeyExpr)
            {
                var keyType = context.Infer(indexKeyExpr);
                if (keyType is EnumInstanceType { EnumType: { } enumType })
                {
                    if (enumType.SubTypeOf(type.GenericArgs[0], context))
                    {
                        return new LuaSymbol(string.Empty, type.GenericArgs[1], new VirtualInfo());
                    }
                }
                else if (keyType.SubTypeOf(type.GenericArgs[0], context))
                {
                    return new LuaSymbol(string.Empty, type.GenericArgs[1], new VirtualInfo());
                }
            }
        }

        return null;
    }
}
