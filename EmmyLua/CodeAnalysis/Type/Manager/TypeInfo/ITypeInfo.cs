using EmmyLua.CodeAnalysis.Document;

namespace EmmyLua.CodeAnalysis.Type.Manager.TypeInfo;

public interface ITypeInfo
{
    public bool RemovePartial(LuaDocumentId documentId);
}
