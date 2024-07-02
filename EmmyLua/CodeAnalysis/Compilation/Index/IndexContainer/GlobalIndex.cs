using EmmyLua.CodeAnalysis.Compilation.Declaration;
using EmmyLua.CodeAnalysis.Document;

namespace EmmyLua.CodeAnalysis.Compilation.Index.IndexContainer;

public class GlobalIndex
{
    public void AddGlobal(LuaDocumentId documentId, string name, LuaDeclaration declaration)
    {
    }

    public LuaDeclaration? QueryGlobal(string name)
    {
        return null;
    }

    public void Remove(LuaDocumentId documentId)
    {
    }
}
