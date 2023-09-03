using LuaLanguageServer.CodeAnalysis.Kind;

namespace LuaLanguageServer.CodeAnalysis.Compile.Parser;

public interface IParser : IMarkerEventContainer
{
    public void Expect(LuaTokenKind kind);

    public void Accept(LuaTokenKind kind);

    public void Bump();

    public LuaTokenKind Current { get; }
}
