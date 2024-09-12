using EmmyLua.CodeAnalysis.Compilation.Declaration;
using EmmyLua.CodeAnalysis.Document;

namespace EmmyLua.CodeAnalysis.Compilation.Type.Types;

public class LuaTypeRef(LuaTypeId id) : LuaType
{
    public LuaTypeId Id { get; } = id;

    public LuaDocumentId DocumentId => Id.Id.DocumentId;
}
