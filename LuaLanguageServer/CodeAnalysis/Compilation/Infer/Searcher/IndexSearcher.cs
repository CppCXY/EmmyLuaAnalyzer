using LuaLanguageServer.CodeAnalysis.Compilation.StubIndex;
using LuaLanguageServer.CodeAnalysis.Compilation.Type;
using LuaLanguageServer.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace LuaLanguageServer.CodeAnalysis.Compilation.Infer.Searcher;

public class IndexSearcher : ILuaSearcher
{
    public bool TrySearchType(string name, SearchContext context, out ILuaType type)
    {
        var elements = context.Compilation.StubIndexImpl.ShortNameIndex.Get(name);
        foreach (var luaShortName in elements)
        {
            switch (luaShortName)
            {
                case LuaShortName.Alias alias:
                {
                    type = context.Infer(alias.AliasSyntax);
                    return true;
                }
                case LuaShortName.Class clazz:
                {
                    type = context.Infer(clazz.ClassSyntax);
                    return true;
                }
                case LuaShortName.Enum enumType:
                {
                    type = context.Infer(enumType.EnumSyntax);
                    return true;
                }
                case LuaShortName.Interface interfaceType:
                {
                    type = context.Infer(interfaceType.InterfaceSyntax);
                    return true;
                }
            }
        }

        type = context.Compilation.Builtin.Unknown;
        return false;
    }

    public IEnumerable<LuaTypeMember> SearchMembers(ILuaType type, SearchContext context)
    {
        switch (type)
        {
            case Class luaClass:
            {
                return SearchClassMembers(luaClass, context);
            }
            default:
            {
                return Enumerable.Empty<LuaTypeMember>();
            }
        }
    }

    private IEnumerable<ClassMember> SearchClassMembers(Class luaClass, SearchContext context)
    {
        var syntaxElement = luaClass.GetSyntaxElement(context);

        if (syntaxElement is not LuaDocClassSyntax luaDocClassSyntax)
        {
            yield break;
        }

        var stubIndexImpl = context.Compilation.StubIndexImpl;
        // ---@class A { aa: number }
        if (luaDocClassSyntax.Body is { } body)
        {
            foreach (var field in body.FieldList)
            {
                var member = context.InferMember(field, () =>
                {
                    return field switch
                    {
                        { NameKey: { } nameKey } => new ClassMember(
                            new IndexKey.String(nameKey.RepresentText), field, luaClass),
                        { StringKey: { } stringKey } => new ClassMember(
                            new IndexKey.String(stringKey.RepresentText), field, luaClass),
                        { IntegerKey: { } integerKey } => new ClassMember(
                            new IndexKey.Integer(integerKey.Value), field, luaClass),
                        { TypeKey: { } typeKey } => new ClassMember(
                            new IndexKey.Ty(context.Infer(typeKey)), field, luaClass),
                        _ => null
                    };
                });

                if (member is not null)
                {
                    yield return member;
                }
            }
        }

        var memberIndex = stubIndexImpl.Members;
        // ---@class A
        // ---@field aa number
        foreach (var classField in memberIndex.Get<LuaMember.ClassDocField>(syntaxElement))
        {
            var member = context.InferMember(classField.ClassDocFieldSyntax, () =>
            {
                var field = classField.ClassDocFieldSyntax;
                return field switch
                {
                    { IntegerField: { } integerField } => new ClassMember(
                        new IndexKey.Integer(integerField.Value), field, luaClass),
                    { StringField: { } stringField } => new ClassMember(
                        new IndexKey.String(stringField.InnerString), field, luaClass),
                    { NameField: { } nameField } => new ClassMember(
                        new IndexKey.String(nameField.RepresentText), field, luaClass),
                    { TypeField: { } typeField } => new ClassMember(
                        new IndexKey.Ty(context.Infer(typeField)), field, luaClass),
                    _ => null
                };
            });

            if (member is not null)
            {
                yield return member;
            }
        }

        // attached node
        var attachedIndex = stubIndexImpl.Attached;
        var attached = attachedIndex.Get<LuaDocAttached.Class>(luaDocClassSyntax).FirstOrDefault()?.Attached;
        if (attached is null) yield break;


    }
}
