namespace LuaLanguageServer.CodeAnalysis.Kind;

public enum VisibilityKind
{
    None,
    Public,
    Protected,
    Private,
    Internal,
    Package,
}

public class VisibilityKindHelper
{
    public static VisibilityKind ToVisibilityKind(ReadOnlySpan<char> visibility)
    {
        return visibility switch
        {
            "public" => VisibilityKind.Public,
            "protected" => VisibilityKind.Protected,
            "private" => VisibilityKind.Private,
            "internal" => VisibilityKind.Internal,
            "package" => VisibilityKind.Package,
            _ => VisibilityKind.None
        };
    }
}
