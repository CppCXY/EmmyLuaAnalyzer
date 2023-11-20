using LuaLanguageServer.CodeAnalysis.Compilation.Infer;
using LuaLanguageServer.CodeAnalysis.Compilation.StubIndex;
using LuaLanguageServer.CodeAnalysis.Compilation.Symbol;
using LuaLanguageServer.CodeAnalysis.Syntax.Node;
using LuaLanguageServer.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace LuaLanguageServer.CodeAnalysis.Compilation.Type;

public class LuaClass : LuaType, ILuaNamedType
{
    public string Name { get; }

    public LuaClass(string name) : base(TypeKind.Class)
    {
        Name = name;
    }

    public IEnumerable<GenericParam> GetGenericParams(SearchContext context)
    {
        // if (GetSyntaxElement(context) is { GenericDeclareList.Params: { } genericParams })
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

    public IEnumerable<ILuaSymbol> GetRawMembers(SearchContext context)
    {
        return context.FindMembers(this);
    }

    public override IEnumerable<ILuaSymbol> GetMembers(SearchContext context)
    {
        return GetRawMembers(context);
    }

    public virtual ILuaType? GetSuper(SearchContext context)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<LuaInterface> GetInterfaces(SearchContext context)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// contains all interfaces
    /// </summary>
    public IEnumerable<LuaInterface> GetAllInterface(SearchContext context)
    {
        throw new NotImplementedException();
    }
}

