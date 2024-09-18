using EmmyLua.CodeAnalysis.Compilation.Analyzer.ResolveAnalyzer;
using EmmyLua.CodeAnalysis.Compilation.Symbol;
using EmmyLua.CodeAnalysis.Compilation.Type;
using EmmyLua.CodeAnalysis.Compilation.Type.TypeInfo;
using EmmyLua.CodeAnalysis.Compilation.Type.Types;
using EmmyLua.CodeAnalysis.Compile.Kind;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace EmmyLua.CodeAnalysis.Compilation.Analyzer.DocAnalyzer;

public class TypeUtil
{

    // private void AnalyzeLuaTableType(LuaDocTableTypeSyntax luaDocTableTypeSyntax)
    // {
    //     var declarations = new List<LuaSymbol>();
    //     declarationContext.TypeManager.AddLocalTypeInfo(luaDocTableTypeSyntax.UniqueId);
    //     if (luaDocTableTypeSyntax.Body is not null)
    //     {
    //         foreach (var field in luaDocTableTypeSyntax.Body.FieldList)
    //         {
    //             if (AnalyzeDocDetailField(field) is { } declaration)
    //             {
    //                 declarations.Add(declaration);
    //             }
    //         }
    //     }
    //
    //     declarationContext.TypeManager.AddElementMembers(luaDocTableTypeSyntax.UniqueId, declarations);
    // }


}
