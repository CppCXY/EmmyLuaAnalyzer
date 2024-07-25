using EmmyLua.CodeAnalysis.Compilation.Declaration;
using EmmyLua.CodeAnalysis.Document;

namespace EmmyLua.CodeAnalysis.Type.Manager;

public class ElementTypeInfo
{
    public LuaDocumentId MainDocumentId { get; set; }

    public LuaType? BaseType { get; set; }

    public Dictionary<string, LuaDeclaration>? Declarations { get; set; }
}
