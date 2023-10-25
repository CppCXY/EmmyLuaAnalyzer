using LuaLanguageServer.CodeAnalysis.Compilation.Infer;
using LuaLanguageServer.CodeAnalysis.Syntax.Node;

namespace LuaLanguageServer.CodeAnalysis.Compilation.Type;

public class Array : LuaType
{
    public ILuaType Base { get; }

    public Array(ILuaType baseTy) : base(TypeKind.Array)
    {
        Base = baseTy;
    }

    public override IEnumerable<ILuaType> GetMembers(SearchContext context) => Enumerable.Empty<ILuaType>();

    public override IEnumerable<ILuaType> IndexMember(IndexKey key, SearchContext context)
    {
        switch (key)
        {
            case IndexKey.Integer:
            {
                yield return Base;
                break;
            }
            case IndexKey.Ty ty:
            {
                if (ty.Value == context.Compilation.Builtin.Integer
                    || ty.Value == context.Compilation.Builtin.Number)
                {
                    yield return Base;
                }

                break;
            }
            default:
            {
                break;
            }
        }
    }
}
