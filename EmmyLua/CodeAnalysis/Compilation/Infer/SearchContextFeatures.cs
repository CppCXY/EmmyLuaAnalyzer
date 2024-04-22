namespace EmmyLua.CodeAnalysis.Compilation.Infer;

public class SearchContextFeatures
{
    public bool Cache { get; set; } = true;

    public bool CacheUnknown { get; set; } = true;

    public bool CacheBaseMember { get; set; } = true;
}
