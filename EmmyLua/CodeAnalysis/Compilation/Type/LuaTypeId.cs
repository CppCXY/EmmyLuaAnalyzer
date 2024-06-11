using EmmyLua.CodeAnalysis.Document;

namespace EmmyLua.CodeAnalysis.Compilation.Type;

// TODO future: use this type
public readonly record struct LuaTypeId(LuaDocumentId DocumentId, int Id);
