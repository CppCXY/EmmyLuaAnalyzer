using LuaLanguageServer.CodeAnalysis.Compilation.Analyzer.Declaration;
using LuaLanguageServer.CodeAnalysis.Compilation.Analyzer.Infer;
using LuaLanguageServer.CodeAnalysis.Compilation.Symbol;

namespace LuaLanguageServer.CodeAnalysis.Compilation.Type;

public class LuaArray : LuaType
{
    public ILuaType Base { get; }

    public VirtualSymbol Symbol { get; }

    public LuaArray(ILuaType baseTy) : base(TypeKind.Array)
    {
        Base = baseTy;
        Symbol = new VirtualSymbol(baseTy, this);
    }

    public override IEnumerable<Declaration> GetMembers(SearchContext context) => Enumerable.Empty<Declaration>();

    public override IEnumerable<Declaration> IndexMember(IndexKey key, SearchContext context)
    {
        // switch (key)
        // {
        //     case IndexKey.Integer:
        //     {
        //         yield return Symbol;
        //         break;
        //     }
        //     case IndexKey.Ty ty:
        //     {
        //         if (ty.Value.SubTypeOf(context.Compilation.Builtin.Number, context))
        //         {
        //             yield return Symbol;
        //         }
        //
        //         break;
        //     }
        // }
        throw new NotImplementedException();
    }
}
