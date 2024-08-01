namespace EmmyLua.CodeAnalysis.Compilation.Search;

public class SearchContextFeatures
{
    public bool Cache { get; set; } = true;

    public bool CacheUnknown { get; set; } = true;

    public bool TableRawInfer { get; set; } = false;
}
