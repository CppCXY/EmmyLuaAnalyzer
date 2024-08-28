using EmmyLua.CodeAnalysis.Document;

namespace EmmyLua.CodeAnalysis.Compilation.Type.TypeInfo;

public interface ITypeInfo
{
    public bool RemovePartial(LuaDocumentId documentId, LuaTypeManager typeManager);
}
