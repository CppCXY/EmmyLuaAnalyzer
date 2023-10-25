using LuaLanguageServer.CodeAnalysis.Compilation.Infer;
using LuaLanguageServer.CodeAnalysis.Compilation.StubIndex;
using LuaLanguageServer.CodeAnalysis.Syntax.Node;
using LuaLanguageServer.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace LuaLanguageServer.CodeAnalysis.Compilation.Type;

public class Class : LuaType, ILuaNamedType
{
    public string Name { get; }

    public Class(string name) : base(TypeKind.Class)
    {
        Name = name;
    }

    public override IEnumerable<ClassMember> GetMembers(SearchContext context)
    {
        var syntaxElement = context.Compilation
            .StubIndexImpl.ShortNameIndex.Get<LuaShortName.Class>(Name).FirstOrDefault()?.ClassSyntax;
        if (syntaxElement is null)
        {
            yield break;
        }

        var memberIndex = context.Compilation.StubIndexImpl.Members;
        foreach (var classField in memberIndex.Get<LuaMember.ClassDocField>(syntaxElement))
        {
            // yield return context.Infer(classField.ClassDocFieldSyntax);
        }

        // TODO attach variable
    }

    public IEnumerable<ILuaType> IndexInteger(long key, SearchContext context)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<ILuaType> IndexMap(ILuaType key, SearchContext context)
    {
        throw new NotImplementedException();
    }

    public override IEnumerable<ILuaType> IndexMember(IndexKey key, SearchContext context)
    {
        switch (key)
        {
            case IndexKey.String str:
            {
                return GetNamedMembers(str.Value, context);
            }
            case IndexKey.Integer integer:
            {
                return IndexInteger(integer.Value, context);
            }
            case IndexKey.Ty ty:
            {
                return IndexMap(ty.Value, context);
            }
        }

        return Enumerable.Empty<ILuaType>();
    }
}
