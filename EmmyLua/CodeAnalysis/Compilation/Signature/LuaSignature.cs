using EmmyLua.CodeAnalysis.Compilation.Symbol;
using EmmyLua.CodeAnalysis.Compilation.Type;
using EmmyLua.CodeAnalysis.Compilation.Type.Types;

namespace EmmyLua.CodeAnalysis.Compilation.Signature;


public class LuaSignature(LuaType? returnType, List<LuaSymbol> parameters, bool colonDefine)
{
    public LuaType? ReturnType { get; set; } = returnType;

    public List<LuaSymbol> Parameters { get; } = parameters;

    public bool ColonDefine { get; } = colonDefine;

    public LuaSignature? NextOverload { get; set; }

    public virtual LuaSignature Instantiate(TypeSubstitution substitution)
    {
        var newReturnType = ReturnType?.Instantiate(substitution);
        var newParameters = Parameters
            .Select(parameter => parameter.Instantiate(substitution))
            .ToList();
        return new LuaSignature(newReturnType, newParameters, ColonDefine);
    }
}
