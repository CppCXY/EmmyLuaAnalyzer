using EmmyLua.CodeAnalysis.Compilation.Search;
using EmmyLua.CodeAnalysis.Compilation.Symbol;
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
        var mainElementId = ElementId;
        if (mainElementId == SyntaxElementId.Empty)
        {
            yield break;
        }

        var document = context.Compilation.Project.GetDocument(mainElementId.DocumentId);
        if (document is not null)
        {
            var element = document.SyntaxTree.GetElement(mainElementId.ElementId);
            if (element is not null)
            {
                yield return element.Location;
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
        if (IsDefinedInDocument(documentId))
        {
            if (Supers is not null)
            {
                var newSupers = new List<LuaNamedType>();
                newSupers.AddRange(Supers);
                foreach (var super in newSupers)
                {
                    var superTypeInfo = typeManager.FindTypeInfo(super);
                    if (superTypeInfo is { SubTypes: { } subTypes })
                    {
                        for (var i = subTypes.Count - 1; i >= 0; i--)
                        {
                            if (typeManager.FindTypeInfo(subTypes[i]) is { } subTypeInfo && subTypeInfo == this)
                            {
                                subTypes.RemoveAt(i);
                                break;
                            }
                        }
                    }
                }
            }

            Supers = null;
            SubTypes = null;
            return true;
        }

        return false;
    }

    public override bool IsDefinedInDocument(LuaDocumentId documentId) => ElementId.DocumentId == documentId;

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
            Implements.TryAdd(luaSymbol.Name, luaSymbol);
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
                Operators.Add(kind, list);
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
