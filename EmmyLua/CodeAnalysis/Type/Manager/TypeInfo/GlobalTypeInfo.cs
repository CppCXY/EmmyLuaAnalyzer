using EmmyLua.CodeAnalysis.Compilation.Declaration;
using EmmyLua.CodeAnalysis.Compilation.Symbol;
using EmmyLua.CodeAnalysis.Document;

namespace EmmyLua.CodeAnalysis.Type.Manager.TypeInfo;

public class GlobalTypeInfo : ITypeInfo
{
    public LuaDocumentId MainDocumentId { get; set; }

    public Dictionary<LuaDocumentId, LuaSymbol> DefinedDeclarations { get; init; } = new();

    public string Name { get; set; } = string.Empty;

    public LuaType? BaseType => DefinedDeclarations.TryGetValue(MainDocumentId, out var declaration) ? declaration.Type : null;

    public Dictionary<string, LuaSymbol>? Declarations { get; set; }

    public bool RemovePartial(LuaDocumentId documentId)
    {
        var removeAll = !RemoveMembers(documentId);
        DefinedDeclarations.Remove(documentId);
        if (DefinedDeclarations.Count != 0)
        {
            MainDocumentId = DefinedDeclarations.Keys.FirstOrDefault();
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
        }

        return removeAll;
    }

    public bool IsDefinedInDocument(LuaDocumentId documentId)
    {
        return DefinedDeclarations.ContainsKey(documentId);
    }
}
