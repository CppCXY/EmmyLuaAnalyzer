namespace EmmyLua.CodeAnalysis.Document.Version;

public record FrameworkVersion(string Name, VersionNumber Version)
{
    public static FrameworkVersion Default = new FrameworkVersion(string.Empty, new VersionNumber(0, 0, 0, 0));

    public static bool TryParse(string text, out FrameworkVersion frameworkVersion)
    {
        frameworkVersion = Default;
        var parts = text.Split(' ');
        if (parts.Length != 2)
        {
            return false;
        }

        try
        {
            var version = VersionNumber.Parse(parts[1]);
            frameworkVersion = new FrameworkVersion(parts[0], version);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
