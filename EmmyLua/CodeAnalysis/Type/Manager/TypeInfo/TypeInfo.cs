using EmmyLua.CodeAnalysis.Compilation.Search;
using EmmyLua.CodeAnalysis.Compilation.Symbol;
using EmmyLua.CodeAnalysis.Document;
using EmmyLua.CodeAnalysis.Syntax.Node;

namespace EmmyLua.CodeAnalysis.Type.Manager.TypeInfo;

public class TypeInfo : ITypeInfo
{
    public LuaDocumentId MainDocumentId { get; set; }

    public bool ResolvedMainDocumentId { get; set; }

    public HashSet<SyntaxElementId> DefinedElementIds { get; init; } = new();

    public HashSet<LuaDocumentId> DefinedDocumentIds { get; init; } = new();

    public SyntaxElementId MainElementId
    {
        get
        {
            if (DefinedElementIds.Count == 0)
            {
                return SyntaxElementId.Empty;
            }
            else if (DefinedElementIds.Count == 1)
            {
                return DefinedElementIds.First();
            }

            return DefinedElementIds.FirstOrDefault(it => it.DocumentId == MainDocumentId);
        }
    }

    public LuaLocation? GetLocation(SearchContext context)
    {
        var mainElementId = MainElementId;
        if (mainElementId == SyntaxElementId.Empty)
        {
            return null;
        }

        var document = context.Compilation.Project.GetDocument(mainElementId.DocumentId);
        if (document is not null)
        {
            var element = document.SyntaxTree.GetElement(mainElementId.ElementId);
            if (element is not null)
            {
                return element.Location;
            }
        }

        return null;
    }

    public string Name { get; set; } = string.Empty;

    public List<LuaSymbol>? GenericParams { get; set; }

    public LuaType? BaseType { get; set; }

    public List<LuaNamedType>? Supers { get; set; }

    public Dictionary<string, LuaSymbol>? Declarations { get; set; }

    public Dictionary<string, LuaSymbol>? Implements { get; set; }

    public Dictionary<TypeOperatorKind, List<TypeOperator>>? Operators { get; set; }

    public record struct OverloadStub(LuaDocumentId DocumentId, LuaMethodType MethodType);

    public List<OverloadStub>? Overloads { get; set; }

    public NamedTypeKind Kind { get; init; } = NamedTypeKind.None;

    public LuaTypeAttribute Attribute { get; init; } = LuaTypeAttribute.None;

    public bool Partial => Attribute.HasFlag(LuaTypeAttribute.Partial);

    public bool Exact => Attribute.HasFlag(LuaTypeAttribute.Exact);

    public bool RemovePartial(LuaDocumentId documentId)
    {
        var removeAll = true;
        if (MainDocumentId == documentId)
        {
            GenericParams = null;
            BaseType = null;
            Supers = null;
            ResolvedMainDocumentId = false;
        }

        if (RemoveMembers(documentId))
        {
            removeAll = false;
        }

        if (RemoveOperators(documentId))
        {
            removeAll = false;
        }

        if (RemoveOverloads(documentId))
        {
            removeAll = false;
        }

        DefinedElementIds.RemoveWhere(it => it.DocumentId == documentId);
        DefinedDocumentIds.Remove(documentId);
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
        }

        if (Implements is not null)
        {
            var toBeRemove = new List<string>();
            foreach (var (key, value) in Implements)
            {
                if (value.DocumentId == documentId)
                {
                    toBeRemove.Add(key);
                }
            }

            foreach (var key in toBeRemove)
            {
                Implements.Remove(key);
            }

            if (Implements.Count == 0)
            {
                Implements = null;
            }
        }

        if (Implements is null && Declarations is null)
        {
            removeAll = false;
        }

        return removeAll;
    }

    private bool RemoveOperators(LuaDocumentId documentId)
    {
        var removeAll = true;
        if (Operators is not null)
        {
            var toBeRemove = new List<TypeOperatorKind>();
            foreach (var (key, value) in Operators)
            {
                for (var i = value.Count - 1; i >= 0; i--)
                {
                    if (value[i].LuaSymbol.DocumentId == documentId)
                    {
                        value.RemoveAt(i);
                    }
                }

                if (value.Count == 0)
                {
                    toBeRemove.Add(key);
                }
            }

            foreach (var key in toBeRemove)
            {
                Operators.Remove(key);
            }

            if (Operators.Count == 0)
            {
                Operators = null;
            }
        }

        return removeAll;
    }

    private bool RemoveOverloads(LuaDocumentId documentId)
    {
        var removeAll = true;
        if (Overloads is not null)
        {
            var overloads = Overloads.Where(it => it.DocumentId != documentId).ToList();
            if (overloads.Count > 0)
            {
                Overloads = overloads;
                removeAll = false;
            }
            else
            {
                Overloads = null;
            }
        }

        return removeAll;
    }

    public bool IsDefinedInDocument(LuaDocumentId documentId)
    {
        return DefinedDocumentIds.Contains(documentId);
    }
}
