using EmmyLua.CodeAnalysis.Compilation.Symbol;
using EmmyLua.CodeAnalysis.Compilation.Type.Types;
using EmmyLua.CodeAnalysis.Document;

namespace EmmyLua.CodeAnalysis.Compilation.Type.TypeInfo;

public class DocumentElementTypeInfo : ITypeInfo
{
    public LuaDocumentId DocumentId { get; set; }

    public LuaType? BaseType { get; set; }

    public Dictionary<string, LuaSymbol>? Declarations { get; set; }

    public bool RemovePartial(LuaDocumentId documentId, LuaTypeManager typeManager)
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
