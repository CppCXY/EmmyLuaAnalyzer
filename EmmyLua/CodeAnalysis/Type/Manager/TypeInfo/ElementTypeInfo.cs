using EmmyLua.CodeAnalysis.Compilation.Declaration;
using EmmyLua.CodeAnalysis.Compilation.Symbol;
using EmmyLua.CodeAnalysis.Document;

namespace EmmyLua.CodeAnalysis.Type.Manager.TypeInfo;

public class DocumentElementTypeInfo : ITypeInfo
{
    public LuaDocumentId DocumentId { get; set; }

    public LuaType? BaseType { get; set; }

    public Dictionary<string, LuaSymbol>? Declarations { get; set; }

    public bool RemovePartial(LuaDocumentId documentId)
    {
        if (DocumentId == documentId)
        {
            BaseType = null;
            Declarations = null;
            return true;
        }

        return false;
    }
}
