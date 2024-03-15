using EmmyLua.CodeAnalysis.Compilation.Infer;
using EmmyLua.CodeAnalysis.Compilation.Symbol;

namespace EmmyLua.CodeAnalysis.Compilation.Semantic.Render;

public class LuaRenderBuilder(SearchContext context)
{
    public string Render(LuaSymbol declaration)
    {
        // return symbol switch
        // {
        //     LuaFunctionSymbol functionSymbol => RenderFunction(functionSymbol),
        //     LuaFieldSymbol fieldSymbol => RenderField(fieldSymbol),
        //     LuaLocalSymbol localSymbol => RenderLocal(localSymbol),
        //     LuaParameterSymbol parameterSymbol => RenderParameter(parameterSymbol),
        //     LuaTypeSymbol typeSymbol => RenderType(typeSymbol),
        //     LuaModuleSymbol moduleSymbol => RenderModule(moduleSymbol),
        //     LuaEnumSymbol enumSymbol => RenderEnum(enumSymbol),
        //     LuaAliasSymbol aliasSymbol => RenderAlias(aliasSymbol),
        //     LuaTableSymbol tableSymbol => RenderTable(tableSymbol),
        //     LuaClassSymbol classSymbol => RenderClass(classSymbol)
        // };
        return "哈哈哈";
    }
}
