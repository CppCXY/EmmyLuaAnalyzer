using EmmyLua.CodeAnalysis.Util.FilenameConverter;

namespace LanguageServer.Completion;

public class CompletionConfig
{
    public bool AutoRequire { get; set; } = true;
    
    public bool CallSnippet { get; set; } = false;
    
    public FilenameConvention AutoRequireFilenameConvention { get; set; } = FilenameConvention.SnakeCase;
}