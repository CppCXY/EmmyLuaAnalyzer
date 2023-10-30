using LuaLanguageServer.CodeAnalysis.Compilation.Infer;
using LuaLanguageServer.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace LuaLanguageServer.CodeAnalysis.Compilation.Type;

public class TableStruct : LuaType
{
    public LuaDocTableTypeSyntax Table { get; }

    public TableStruct(LuaDocTableTypeSyntax table) : base(TypeKind.Table)
    {
        Table = table;
    }

    public override IEnumerable<LuaTypeMember> GetMembers(SearchContext context)
    {
        foreach (var field in Table.FieldList)
        {
            var member = context.InferMember(field, () =>
            {
                return field switch
                {
                    { IsIntegerKey: true, IntegerKey: { } integerKey } => new TableStructMember(
                        new IndexKey.Integer(integerKey.IntegerValue), field, this),
                    { IsStringKey: true, StringKey: { } stringKey } => new TableStructMember(
                        new IndexKey.String(stringKey.InnerString), field, this),
                    { IsTypeKey: true, TypeKey: { } typeKey } => new TableStructMember(
                        new IndexKey.Ty(context.Infer(typeKey)), field, this),
                    { IsNameKey: true, NameKey: { } nameKey } => new TableStructMember(
                        new IndexKey.String(nameKey.RepresentText), field, this),
                    _ => null
                };
            });

            if (member is not null)
            {
                yield return member;
            }
        }
    }
}

public class TableStructMember : LuaTypeMember
{
    public IndexKey Key { get; }

    public LuaDocTypedFieldSyntax Field { get; }

    public TableStructMember(IndexKey key, LuaDocTypedFieldSyntax field, TableStruct? containingType) : base(
        containingType)
    {
        Key = key;
        Field = field;
    }

    public override ILuaType GetType(SearchContext context)
    {
        return context.Infer(Field.Type);
    }

    public override bool MatchKey(IndexKey key, SearchContext context)
    {
        return (key, Key) switch
        {
            (IndexKey.Integer i1, IndexKey.Integer i2) => i1.Value == i2.Value,
            (IndexKey.String s1, IndexKey.String s2) => s1.Value == s2.Value,
            (IndexKey.Ty t1, IndexKey.Ty t2) => t1.Value == t2.Value,
            _ => false
        };
    }
}
