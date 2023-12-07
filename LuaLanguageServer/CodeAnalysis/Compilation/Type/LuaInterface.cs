using LuaLanguageServer.CodeAnalysis.Compilation.Analyzer.Infer;
using LuaLanguageServer.CodeAnalysis.Compilation.StubIndex;
using LuaLanguageServer.CodeAnalysis.Compilation.Symbol;
using LuaLanguageServer.CodeAnalysis.Syntax.Node;
using LuaLanguageServer.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace LuaLanguageServer.CodeAnalysis.Compilation.Type;

public class LuaInterface : LuaType, ILuaNamedType
{
    public string Name { get; }

    public LuaInterface(string name) : base(TypeKind.Interface)
    {
        Name = name;
    }

    public IEnumerable<GenericParam> GetGenericParams(SearchContext context)
    {
        // if (GetSyntaxElement(context) is LuaDocInterfaceSyntax { GenericDeclareList.Params: { } genericParams })
        // {
        //     foreach (var genericParam in genericParams)
        //     {
        //         if (genericParam is { Name: { } name })
        //         {
        //             yield return new GenericParam(name.RepresentText, context.Infer(genericParam.Type), genericParam);
        //         }
        //     }
        // }
        throw new NotImplementedException();
    }

    public override IEnumerable<ILuaSymbol> GetMembers(SearchContext context)
    {
        return context.FindMembers(this);
    }
}
