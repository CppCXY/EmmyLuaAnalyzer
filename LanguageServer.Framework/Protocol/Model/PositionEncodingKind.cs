namespace EmmyLua.LanguageServer.Framework.Protocol.Model;

public static class PositionEncodingKind
{
    /**
     * Character offsets count UTF-8 code units (i.e. bytes).
     */
    // ReSharper disable once InconsistentNaming
    public const string UTF8 = "utf-8";
        
    /**
     * Character offsets count UTF-16 code units.
     *
     * This is the default and must always be supported
     * by servers.
     */
    // ReSharper disable once InconsistentNaming
    public const string UTF16 = "utf-16";
    
    /**
     * Character offsets count UTF-32 code units.
     *
     * Implementation note: these are the same as Unicode code points,
     * so this `PositionEncodingKind` may also be used for an
     * encoding-agnostic representation of character offsets.
     */
    // ReSharper disable once InconsistentNaming
    public const string UTF32 = "utf-32";
}