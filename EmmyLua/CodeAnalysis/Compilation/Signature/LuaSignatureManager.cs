using EmmyLua.CodeAnalysis.Container;
using EmmyLua.CodeAnalysis.Document;

namespace EmmyLua.CodeAnalysis.Compilation.Signature;

public class LuaSignatureManager
{
    private InFileIndex<LuaSignatureId, LuaSignature> Signatures { get; } = new();

    public void AddSignature(LuaSignatureId id, LuaSignature signature)
    {
        Signatures.Add(id.Id.DocumentId, id, signature);
    }

    public LuaSignature? GetSignature(LuaSignatureId id)
    {
        return Signatures.Query(id);
    }

    public void Remove(LuaDocumentId id)
    {
        Signatures.Remove(id);
    }
}
