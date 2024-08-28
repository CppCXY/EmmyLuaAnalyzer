using EmmyLua.CodeAnalysis.Compilation.Symbol;
using EmmyLua.CodeAnalysis.Compilation.Type.Types;

namespace EmmyLua.CodeAnalysis.Compilation.Signature;

public class LuaGenericSignature(LuaType returnType, List<LuaSymbol> parameters, bool colonDefine, List<LuaSymbol> genericParameters)
    : LuaSignature(returnType, parameters, colonDefine)
{
    public List<LuaSymbol> GenericParameters { get; } = genericParameters;
}
