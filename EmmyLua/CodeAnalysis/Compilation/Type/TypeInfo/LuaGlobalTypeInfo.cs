using EmmyLua.CodeAnalysis.Compilation.Search;
using EmmyLua.CodeAnalysis.Compilation.Symbol;
using EmmyLua.CodeAnalysis.Compilation.Type.Types;
using EmmyLua.CodeAnalysis.Document;
using EmmyLua.CodeAnalysis.Syntax.Node;

namespace EmmyLua.CodeAnalysis.Compilation.Type.TypeInfo;

public class LuaGlobalTypeInfo(SyntaxElementId elementId, NamedTypeKind kind, LuaTypeAttribute attribute) : LuaTypeInfo
{
    private LuaDocumentId _mainDocumentId = LuaDocumentId.VirtualDocumentId;

    private Dictionary<LuaDocumentId, LuaSymbol> _symbolSet = new();

    private HashSet<LuaDocumentId> _documentIdSet = new();

    public void AddDefineSymbol(LuaSymbol luaSymbol)
    {
        _symbolSet[luaSymbol.DocumentId] = luaSymbol;
        _documentIdSet.Add(luaSymbol.DocumentId);
    }

    public LuaSymbol? MainLuaSymbol
    {
        get
        {
            if (_symbolSet.TryGetValue(_mainDocumentId, out var symbol))
            {
                return symbol;
            }

            return _symbolSet.FirstOrDefault().Value;
        }
    }

    public override bool Remove(LuaDocumentId documentId, LuaTypeManager typeManager)
    {
        if (documentId == _mainDocumentId)
        {
            BaseType = null;
        }

        var removeAll = RemoveMembers(documentId);
        _documentIdSet.Remove(documentId);
        _symbolSet.Remove(documentId);
        if (_documentIdSet.Count != 0)
        {
            _mainDocumentId = _documentIdSet.FirstOrDefault();
            removeAll = false;
        }

        return removeAll;
    }

    private bool RemoveMembers(LuaDocumentId documentId)
    {
        var removeAll = true;
        if (Declarations is not null)
        {
            var toBeRemove = new List<string>();
            foreach (var (key, value) in Declarations)
            {
                if (value.DocumentId == documentId)
                {
                    toBeRemove.Add(key);
                }
            }

            foreach (var key in toBeRemove)
            {
                Declarations.Remove(key);
            }

            if (Declarations.Count == 0)
            {
                Declarations = null;
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
        var location = MainLuaSymbol?.GetLocation(context);
        if (location is not null)
        {
            yield return location;
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

    public override bool IsDefinedInDocument(LuaDocumentId documentId)
    {
        return _documentIdSet.Contains(documentId);
    }

    public override void AddDeclaration(LuaSymbol luaSymbol)
    {
        Declarations ??= new();
        Declarations.TryAdd(luaSymbol.Name, luaSymbol);
        _documentIdSet.Add(luaSymbol.DocumentId);
    }

    public override void AddImplement(LuaSymbol luaSymbol)
    {
        Implements ??= new();
        Implements.TryAdd(luaSymbol.Name, luaSymbol);
        _documentIdSet.Add(luaSymbol.DocumentId);
    }

    public override void AddSuper(LuaNamedType super)
    {
        Supers ??= new();
        Supers.Add(super);
        _documentIdSet.Add(super.DocumentId);
    }

    public override void AddSubType(LuaNamedType subType)
    {
        SubTypes ??= new();
        SubTypes.Add(subType);
    }

    public override void AddOperator(TypeOperatorKind kind, TypeOperator typeOperator)
    {
        Operators ??= new();
        if (!Operators.TryGetValue(kind, out var list))
        {
            list = new List<TypeOperator>();
            Operators[kind] = list;
        }

        list.Add(typeOperator);
        _documentIdSet.Add(typeOperator.Id.DocumentId);
    }

    public override void AddGenericParameter(LuaSymbol genericParameter)
    {
        GenericParameters ??= new();
        GenericParameters.Add(genericParameter);
        _documentIdSet.Add(genericParameter.DocumentId);
    }
}
