using EmmyLua.CodeAnalysis.Compilation.Declaration;

namespace EmmyLua.CodeAnalysis.Compilation.Type.Types;

public class LuaTypeRef(TypeId id) : LuaType
{
    public TypeId Id { get; } = id;
}
