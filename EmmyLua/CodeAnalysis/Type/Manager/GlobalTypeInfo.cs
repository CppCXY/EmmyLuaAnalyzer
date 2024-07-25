using EmmyLua.CodeAnalysis.Compilation.Declaration;
using EmmyLua.CodeAnalysis.Document;
using EmmyLua.CodeAnalysis.Syntax.Node;

namespace EmmyLua.CodeAnalysis.Type.Manager;

public class GlobalTypeInfo
{
    public LuaDocumentId MainDocumentId { get; set; }

    public HashSet<SyntaxElementId> DefinedElementIds { get; set; } = new();

    public string Name { get; set; } = string.Empty;

    public LuaType? BaseType { get; set; }

    public Dictionary<string, LuaDeclaration>? Declarations { get; set; }

    public bool RemovePartial(LuaDocumentId documentId)
    {
        var removeAll = true;
        if (MainDocumentId == documentId)
        {
            BaseType = null;
        }

        if (RemoveMembers(documentId))
        {
            removeAll = false;
        }

        DefinedElementIds.RemoveWhere(it => it.DocumentId == documentId);
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
        return MainDocumentId == documentId || DefinedElementIds.Any(it => it.DocumentId == documentId);
    }
}
