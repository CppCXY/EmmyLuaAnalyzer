using EmmyLua.CodeAnalysis.Compilation.Search;
using EmmyLua.CodeAnalysis.Compilation.Symbol;
using EmmyLua.CodeAnalysis.Compilation.Type.TypeCompute;
using EmmyLua.CodeAnalysis.Compilation.Type.Types;
using EmmyLua.CodeAnalysis.Document;
using EmmyLua.CodeAnalysis.Syntax.Node;

namespace EmmyLua.CodeAnalysis.Compilation.Type.TypeInfo;

public class LuaGlobalTypeInfo(NamedTypeKind kind, LuaTypeAttribute attribute) : LuaTypeInfo
{
    private Dictionary<LuaDocumentId, SyntaxElementId> _elementIds = new();

    private HashSet<LuaDocumentId> _documentIdSet = new();

    public override void AddDefineId(SyntaxElementId id)
    {
        _elementIds[id.DocumentId] = id;
        _documentIdSet.Add(id.DocumentId);
    }

    public override bool Remove(LuaDocumentId documentId, LuaTypeManager typeManager)
    {
        var removeAll = RemoveMembers(documentId);
        _documentIdSet.Remove(documentId);
        _elementIds.Remove(documentId);
        if (_documentIdSet.Count != 0)
        {
            removeAll = false;
        }

        return removeAll;
    }

    private bool RemoveMembers(LuaDocumentId documentId)
    {
        var removeAll = true;
        if (_declarations is not null)
        {
            var toBeRemove = new List<string>();
            foreach (var (key, value) in _declarations)
            {
                if (value.DocumentId == documentId)
                {
                    toBeRemove.Add(key);
                }
            }

            foreach (var key in toBeRemove)
            {
                _declarations.Remove(key);
            }

            if (_declarations.Count == 0)
            {
                _declarations = null;
            }
            else
            {
                removeAll = false;
            }
        }

        return removeAll;
    }

    public override IEnumerable<LuaLocation> GetLocation(SearchContext context)
    {
        foreach (var id in _elementIds.Values)
        {
            if (id.GetLocation(context) is { } location)
            {
                yield return location;
            }
        }
    }

    private List<LuaSymbol>? _genericParameters = null;

    public override List<LuaSymbol>? GenericParameters => _genericParameters;

    private LuaType? _baseType = null;

    public override LuaType? BaseType => _baseType;

    public override TypeComputer? TypeCompute => null;

    private List<LuaTypeRef>? _supers = null;

    public override List<LuaTypeRef>? Supers => _supers;

    private  Dictionary<string, LuaSymbol>? _declarations = null;

    public override Dictionary<string, LuaSymbol>? Declarations => _declarations;

    private Dictionary<string, LuaSymbol>? _implements = null;

    public override Dictionary<string, LuaSymbol>? Implements => _implements;

    private Dictionary<TypeOperatorKind, List<TypeOperator>>? _operators = null;

    public override Dictionary<TypeOperatorKind, List<TypeOperator>>? Operators => _operators;

    public override NamedTypeKind Kind { get; } = kind;

    public override LuaTypeAttribute Attribute { get; } = attribute;

    public override bool IsDefinedInDocument(LuaDocumentId documentId)
    {
        return _documentIdSet.Contains(documentId);
    }

    public override void AddDeclaration(LuaSymbol luaSymbol)
    {
        _declarations ??= new();
        _declarations.TryAdd(luaSymbol.Name, luaSymbol);
        _documentIdSet.Add(luaSymbol.DocumentId);
    }

    public override void AddImplement(LuaSymbol luaSymbol)
    {
        _implements ??= new();
        _implements.TryAdd(luaSymbol.Name, luaSymbol);
        _documentIdSet.Add(luaSymbol.DocumentId);
    }

    public override void AddSuper(LuaTypeRef super)
    {
        _supers ??= new();
        _supers.Add(super);
        _documentIdSet.Add(super.DocumentId);
    }

    public override void AddOperator(TypeOperatorKind kind, TypeOperator typeOperator)
    {
        _operators ??= new();
        if (!_operators.TryGetValue(kind, out var list))
        {
            list = new List<TypeOperator>();
            _operators[kind] = list;
        }

        list.Add(typeOperator);
        _documentIdSet.Add(typeOperator.Id.DocumentId);
    }

    public override void AddGenericParameter(LuaSymbol genericParameter)
    {
        _genericParameters ??= new();
        _genericParameters.Add(genericParameter);
        _documentIdSet.Add(genericParameter.DocumentId);
    }

    public override void AddBaseType(LuaType baseType)
    {
        _baseType = baseType;
    }
}
