namespace LanguageServer.InlayHint;

public class InlayHintConfig
{
    public bool ParamHint { get; set; } = true;
    
    public bool IndexHint { get; set; } = true;
    
    public bool LocalHint { get; set; } = false;
    
    public bool OverrideHint { get; set; } = true;
}