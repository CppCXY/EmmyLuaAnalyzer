using LuaLanguageServer.CodeAnalysis.Compilation.Infer;
using LuaLanguageServer.CodeAnalysis.Compilation.StubIndex;

namespace LuaLanguageServer.CodeAnalysis.Compilation.Type;

public class Class : LuaType, ILuaNamedType
{
    public string Name { get; }

    public Class(string name) : base(TypeKind.Class)
    {
        Name = name;
    }

    public override IEnumerable<ILuaType> GetMembers(SearchContext context)
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
            yield return context.Infer(classField.ClassDocFieldSyntax);
        }

        // TODO attach variable
    }
}
