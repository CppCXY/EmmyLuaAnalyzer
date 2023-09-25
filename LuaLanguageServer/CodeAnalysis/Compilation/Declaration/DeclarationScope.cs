namespace LuaLanguageServer.CodeAnalysis.Compilation.Declaration;

public class DeclarationScope
{

}

internal class DeclarationNode
{
    public DeclarationNode? Prev { get; set; } = null;

    public DeclarationNode? Next { get; set; } = null;

    public int Position { get; set; }
}

internal abstract class DeclarationNodeContainer
{

}
