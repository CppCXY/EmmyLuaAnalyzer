using LuaLanguageServer.CodeAnalysis.Compilation.StubIndex;
using LuaLanguageServer.CodeAnalysis.Compilation.Symbol;
using LuaLanguageServer.CodeAnalysis.Compilation.Type;
using LuaLanguageServer.CodeAnalysis.Syntax.Node;
using LuaLanguageServer.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace LuaLanguageServer.CodeAnalysis.Compilation.Infer.Searcher;

public class IndexSearcher : LuaSearcher
{
    public bool TrySearchClass(string name, SearchContext context, out LuaClass? type)
    {
        // var element = context.Compilation
        //     .StubIndexImpl.ShortNameIndex.Get<LuaShortName.Class>(name).FirstOrDefault()?.ClassSyntax;
        // if(element is {} luaClassSyntax)
        // {
        //     type = context.Infer(luaClassSyntax) as LuaClass;
        //     return true;
        // }

        type = null;
        return false;
    }


    public IEnumerable<LuaSymbol> SearchMembers(ILuaType type, SearchContext context)
    {
        // switch (type)
        // {
        //     case LuaClass luaClass:
        //     {
        //         return SearchClassMembers(luaClass, context);
        //     }
        //     default:
        //     {
        //         return Enumerable.Empty<LuaSymbol>();
        //     }
        // }
        throw new NotImplementedException();
    }

    // private IEnumerable<ClassMember> SearchClassMembers(LuaClass luaLuaClass, SearchContext context)
    // {
    //     var syntaxElement = luaLuaClass.GetSyntaxElement(context);
    //
    //     if (syntaxElement is not LuaDocClassSyntax luaDocClassSyntax)
    //     {
    //         yield break;
    //     }
    //
    //     var stubIndexImpl = context.Compilation.StubIndexImpl;
    //     // ---@class A { aa: number }
    //     if (luaDocClassSyntax.Body is { } body)
    //     {
    //         foreach (var field in body.FieldList)
    //         {
    //             var member = context.InferMember(field, () =>
    //             {
    //                 return field switch
    //                 {
    //                     { NameKey: { } nameKey } => new ClassMember(
    //                         new IndexKey.String(nameKey.RepresentText), field, luaLuaClass),
    //                     { StringKey: { } stringKey } => new ClassMember(
    //                         new IndexKey.String(stringKey.RepresentText), field, luaLuaClass),
    //                     { IntegerKey: { } integerKey } => new ClassMember(
    //                         new IndexKey.Integer(integerKey.Value), field, luaLuaClass),
    //                     { TypeKey: { } typeKey } => new ClassMember(
    //                         new IndexKey.Ty(context.Infer(typeKey)), field, luaLuaClass),
    //                     _ => null
    //                 };
    //             });
    //
    //             if (member is not null)
    //             {
    //                 yield return member;
    //             }
    //         }
    //     }
    //
    //     var memberIndex = stubIndexImpl.Members;
    //     // ---@class A
    //     // ---@field aa number
    //     foreach (var classField in memberIndex.Get<LuaMember.ClassDocField>(syntaxElement))
    //     {
    //         var member = context.InferMember(classField.ClassDocFieldSyntax, () =>
    //         {
    //             var field = classField.ClassDocFieldSyntax;
    //             return field switch
    //             {
    //                 { IntegerField: { } integerField } => new ClassMember(
    //                     new IndexKey.Integer(integerField.Value), field, luaLuaClass),
    //                 { StringField: { } stringField } => new ClassMember(
    //                     new IndexKey.String(stringField.InnerString), field, luaLuaClass),
    //                 { NameField: { } nameField } => new ClassMember(
    //                     new IndexKey.String(nameField.RepresentText), field, luaLuaClass),
    //                 { TypeField: { } typeField } => new ClassMember(
    //                     new IndexKey.Ty(context.Infer(typeField)), field, luaLuaClass),
    //                 _ => null
    //             };
    //         });
    //
    //         if (member is not null)
    //         {
    //             yield return member;
    //         }
    //     }
    //
    //     // attached node
    //     var attachedIndex = stubIndexImpl.Attached;
    //     var attached = attachedIndex.Get<LuaDocAttached.Class>(luaDocClassSyntax).FirstOrDefault()?.Attached;
    //     if (attached is null) yield break;
    // }

}
