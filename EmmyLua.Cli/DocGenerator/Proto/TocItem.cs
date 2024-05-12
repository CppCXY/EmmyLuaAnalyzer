using YamlDotNet.Serialization;

namespace EmmyLua.Cli.DocGenerator.Proto;

public class TocItem
{
    [YamlMember(Alias = "name")]
    public string Name { get; set; } = string.Empty;
    
    [YamlMember(Alias = "href")]
    public string Href { get; set; } = string.Empty;
}
