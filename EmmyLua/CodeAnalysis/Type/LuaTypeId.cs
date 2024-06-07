using EmmyLua.CodeAnalysis.Document;

namespace EmmyLua.CodeAnalysis.Type;

// TODO future: use this type
public readonly record struct LuaTypeId(LuaDocumentId DocumentId, int Id);
