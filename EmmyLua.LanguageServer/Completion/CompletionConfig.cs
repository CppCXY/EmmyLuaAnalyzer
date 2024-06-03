using EmmyLua.CodeAnalysis.Workspace.Module.FilenameConverter;

namespace EmmyLua.LanguageServer.Completion;

public class CompletionConfig
{
    public bool AutoRequire { get; set; } = true;
    
    public bool CallSnippet { get; set; } = false;
    
    public FilenameConvention AutoRequireFilenameConvention { get; set; } = FilenameConvention.SnakeCase;
}