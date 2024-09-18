using EmmyLua.CodeAnalysis.Compilation.Search;
using EmmyLua.CodeAnalysis.Compilation.Symbol;
using EmmyLua.CodeAnalysis.Compilation.Type.TypeCompute;
using EmmyLua.CodeAnalysis.Compilation.Type.Types;
using EmmyLua.CodeAnalysis.Document;
using EmmyLua.CodeAnalysis.Syntax.Node;

namespace EmmyLua.CodeAnalysis.Compilation.Type.TypeInfo;

public class LuaLocalTypeInfo(SyntaxElementId elementId, NamedTypeKind kind, LuaTypeAttribute attribute)
    : LuaTypeInfo
{
    private SyntaxElementId ElementId { get; } = elementId;

    public override IEnumerable<LuaLocation> GetLocation(SearchContext context)
    {
        if (ElementId.GetLocation(context) is { } location)
        {
            yield return location;
        }
    }

    private List<LuaSymbol>? _genericParameters = null;

    public override List<LuaSymbol>? GenericParameters => _genericParameters;

    private LuaType? _baseType = null;

    public override LuaType? BaseType => _baseType;

    public override TypeComputer? TypeCompute => null;

    private List<LuaTypeRef>? _supers = null;

    public override List<LuaTypeRef>? Supers => _supers;

    private Dictionary<string, LuaSymbol> _declarations = new();

    public override Dictionary<string, LuaSymbol>? Declarations => _declarations;

    private Dictionary<string, LuaSymbol>? _implements = null;

    public override Dictionary<string, LuaSymbol>? Implements => _implements;

    private Dictionary<TypeOperatorKind, List<TypeOperator>>? _operators = null;

    public override Dictionary<TypeOperatorKind, List<TypeOperator>>? Operators => _operators;

    public override NamedTypeKind Kind { get; } = kind;

    public override LuaTypeAttribute Attribute { get; } = attribute;

    public override void AddDefineId(SyntaxElementId _)
    {
    }

    public override bool Remove(LuaDocumentId documentId, LuaTypeManager typeManager)
    {
        if (IsDefinedInDocument(documentId))
        {
            // if (Supers is not null)
            // {
            //     var newSupers = new List<LuaNamedType>();
            //     newSupers.AddRange(Supers);
            //     foreach (var super in newSupers)
            //     {
            //         var superTypeInfo = typeManager.FindTypeInfo(super);
            //         if (superTypeInfo is { SubTypes: { } subTypes })
            //         {
            //             for (var i = subTypes.Count - 1; i >= 0; i--)
            //             {
            //                 if (typeManager.FindTypeInfo(subTypes[i]) is { } subTypeInfo && subTypeInfo == this)
            //                 {
            //                     subTypes.RemoveAt(i);
            //                     break;
            //                 }
            //             }
            //         }
            //     }
            // }
            //
            // Supers = null;
            // SubTypes = null;
            return true;
        }

        return false;
    }

    public override bool IsDefinedInDocument(LuaDocumentId documentId) => ElementId.DocumentId == documentId;

    public override void AddDeclaration(LuaSymbol luaSymbol)
    {
        if (IsDefinedInDocument(luaSymbol.DocumentId))
        {
            _declarations ??= new();
            _declarations.TryAdd(luaSymbol.Name, luaSymbol);
        }
    }

    public override void AddImplement(LuaSymbol luaSymbol)
    {
        if (IsDefinedInDocument(luaSymbol.DocumentId))
        {
            AddDeclaration(luaSymbol);
            _implements ??= new();
            _implements.TryAdd(luaSymbol.Name, luaSymbol);
        }
    }

    public override void AddSuper(LuaTypeRef super)
    {
        if (IsDefinedInDocument(super.DocumentId))
        {
            _supers ??= new();
            _supers.Add(super);
        }
    }

    // public override void AddSubType(LuaNamedType subType)
    // {
    //     SubTypes ??= new();
    //     SubTypes.Add(subType);
    // }

    public override void AddOperator(TypeOperatorKind kind, TypeOperator typeOperator)
    {
        if (IsDefinedInDocument(typeOperator.Id.DocumentId))
        {
            _operators ??= new();
            if (!_operators.TryGetValue(kind, out var list))
            {
                list = new();
                _operators.Add(kind, list);
            }

            list.Add(typeOperator);
        }
    }

    public override void AddGenericParameter(LuaSymbol genericParameter)
    {
        if (IsDefinedInDocument(genericParameter.DocumentId))
        {
            _genericParameters ??= new();
            _genericParameters.Add(genericParameter);
        }
    }

    public override void AddBaseType(LuaType baseType)
    {
         _baseType = baseType;
    }
}
