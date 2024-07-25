namespace EmmyLua.CodeAnalysis.Type.Manager;

public class NamespaceIndex
{
    public string FullName { get; init; } = string.Empty;

    public List<string> UsingNamespaces { get; } = new();
}
