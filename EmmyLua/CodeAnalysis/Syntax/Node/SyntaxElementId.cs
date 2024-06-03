using EmmyLua.CodeAnalysis.Document;

namespace EmmyLua.CodeAnalysis.Syntax.Node;

public readonly record struct SyntaxElementId(LuaDocumentId DocumentId, int ElementId)
{
    public static readonly SyntaxElementId Empty = new SyntaxElementId(new LuaDocumentId(0), 0);

    public static SyntaxElementId From(string idString)
    {
        var longId = long.Parse(idString);
        return new SyntaxElementId(new LuaDocumentId((int)(longId >> 32)), (int)longId);
    }

    public long UniqueId => ((long)DocumentId.Id << 32) | (uint)ElementId;

    public string Stringify => UniqueId.ToString();

    public override string ToString()
    {
        return Stringify;
    }
}
