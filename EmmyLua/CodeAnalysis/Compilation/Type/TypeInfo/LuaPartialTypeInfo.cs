using EmmyLua.CodeAnalysis.Compilation.Search;
using EmmyLua.CodeAnalysis.Compilation.Symbol;
using EmmyLua.CodeAnalysis.Compilation.Type.Types;
using EmmyLua.CodeAnalysis.Document;
using EmmyLua.CodeAnalysis.Syntax.Node;

namespace EmmyLua.CodeAnalysis.Compilation.Type.TypeInfo;

public class LuaPartialTypeInfo(NamedTypeKind kind, LuaTypeAttribute attribute) : LuaTypeInfo
{
    private Dictionary<LuaDocumentId, SyntaxElementId> _elementIds = new();

    public override void AddDefineId(SyntaxElementId id)
    {
        _elementIds[id.DocumentId] = id;
    }

    public override IEnumerable<LuaLocation> GetLocation(SearchContext context)
    {
        foreach (var elementId in _elementIds.Values)
        {
            if (elementId.GetLocation(context) is { } location)
            {
                yield return location;
            }
        }
    }

    public override List<LuaSymbol>? GenericParameters { get; protected set; }

    public override LuaType? BaseType { get; protected set; }

    public override List<LuaNamedType>? Supers { get; protected set; }

    public override List<LuaNamedType>? SubTypes { get; protected set; }

    public override Dictionary<string, LuaSymbol>? Declarations { get; protected set; }

    public override Dictionary<string, LuaSymbol>? Implements { get; protected set; }

    public override Dictionary<TypeOperatorKind, List<TypeOperator>>? Operators { get; protected set; }

    public override NamedTypeKind Kind { get; } = kind;

    public override LuaTypeAttribute Attribute { get; } = attribute;

    public override bool Remove(LuaDocumentId documentId, LuaTypeManager typeManager)
    {
        throw new NotImplementedException();
    }

    public override bool IsDefinedInDocument(LuaDocumentId documentId)
    {
        return _elementIds.ContainsKey(documentId);
    }

    public override void AddDeclaration(LuaSymbol luaSymbol)
    {
        if (IsDefinedInDocument(luaSymbol.DocumentId))
        {
            Declarations ??= new();
            Declarations.TryAdd(luaSymbol.Name, luaSymbol);
        }
    }

    public override void AddImplement(LuaSymbol luaSymbol)
    {
        if (IsDefinedInDocument(luaSymbol.DocumentId))
        {
            AddDeclaration(luaSymbol);
            Implements ??= new();
            Implements[luaSymbol.Name] = luaSymbol;
        }
    }

    public override void AddSuper(LuaNamedType super)
    {
        if (IsDefinedInDocument(super.DocumentId))
        {
            Supers ??= new();
            Supers.Add(super);
        }
    }

    public override void AddSubType(LuaNamedType subType)
    {
        SubTypes ??= new();
        SubTypes.Add(subType);
    }

    public override void AddOperator(TypeOperatorKind kind, TypeOperator typeOperator)
    {
        if (IsDefinedInDocument(typeOperator.Id.DocumentId))
        {
            Operators ??= new();
            if (!Operators.TryGetValue(kind, out var list))
            {
                list = new();
                Operators[kind] = list;
            }

            list.Add(typeOperator);
        }
    }

    public override void AddGenericParameter(LuaSymbol genericParameter)
    {
        if (IsDefinedInDocument(genericParameter.DocumentId))
        {
            GenericParameters ??= new();
            GenericParameters.Add(genericParameter);
        }
    }
}

// public void RemoveInherits(LuaTypeManager typeManager)
//     // {
//     //     if (Supers is not null)
//     //     {
//     //         var newSupers = new List<LuaNamedType>();
//     //         newSupers.AddRange(Supers);
//     //         foreach (var super in newSupers)
//     //         {
//     //             var superTypeInfo = typeManager.FindTypeInfo(super);
//     //             if (superTypeInfo is { SubTypes: { } subTypes })
//     //             {
//     //                 for (var i = subTypes.Count - 1; i >= 0; i--)
//     //
//     //                 {
//     //                     if (typeManager.FindTypeInfo(subTypes[i]) is { } subTypeInfo && subTypeInfo == this)
//     //                     {
//     //                         subTypes.RemoveAt(i);
//     //                         break;
//     //                     }
//     //                 }
//     //             }
//     //         }
//     //     }
//     //
//     //     Supers = null;
//     //     SubTypes = null;
//     // }
//
//     private bool RemoveMembers(LuaDocumentId documentId)
//     {
//         var removeAll = false;
//         if (Declarations is not null)
//         {
//             var toBeRemove = new List<string>();
//             foreach (var (key, value) in Declarations)
//             {
//                 if (value.DocumentId == documentId)
//                 {
//                     toBeRemove.Add(key);
//                 }
//             }
//
//             foreach (var key in toBeRemove)
//             {
//                 Declarations.Remove(key);
//             }
//
//             if (Declarations.Count == 0)
//             {
//                 Declarations = null;
//             }
//         }
//
//         if (Implements is not null)
//         {
//             var toBeRemove = new List<string>();
//             foreach (var (key, value) in Implements)
//             {
//                 if (value.DocumentId == documentId)
//                 {
//                     toBeRemove.Add(key);
//                 }
//             }
//
//             foreach (var key in toBeRemove)
//             {
//                 Implements.Remove(key);
//             }
//
//             if (Implements.Count == 0)
//             {
//                 Implements = null;
//             }
//         }
//
//         if (Implements is null && Declarations is null)
//         {
//             removeAll = true;
//         }
//
//         return removeAll;
//     }
//
//     private bool RemoveOperators(LuaDocumentId documentId)
//     {
//         var removeAll = true;
//         if (Operators is not null)
//         {
//             var toBeRemove = new List<TypeOperatorKind>();
//             foreach (var (key, value) in Operators)
//             {
//                 for (var i = value.Count - 1; i >= 0; i--)
//                 {
//                     if (value[i].Id.DocumentId == documentId)
//                     {
//                         value.RemoveAt(i);
//                     }
//                 }
//
//                 if (value.Count == 0)
//                 {
//                     toBeRemove.Add(key);
//                 }
//             }
//
//             foreach (var key in toBeRemove)
//             {
//                 Operators.Remove(key);
//             }
//
//             if (Operators.Count == 0)
//             {
//                 Operators = null;
//             }
//             else
//             {
//                 removeAll = false;
//             }
//         }
//
//         return removeAll;
//     }
//
//     private bool RemoveOverloads(LuaDocumentId documentId)
//     {
//         var removeAll = true;
//         // if (Overloads is not null)
//         // {
//         //     var overloads = Overloads.Where(it => it.DocumentId != documentId).ToList();
//         //     if (overloads.Count > 0)
//         //     {
//         //         Overloads = overloads;
//         //         removeAll = false;
//         //     }
//         //     else
//         //     {
//         //         Overloads = null;
//         //     }
//         // }
//
//         return removeAll;
//     }
