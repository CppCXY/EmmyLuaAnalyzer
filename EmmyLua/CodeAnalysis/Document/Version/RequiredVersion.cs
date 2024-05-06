namespace EmmyLua.CodeAnalysis.Document.Version;

public enum RequiredVersionAction
{
    Equal,
    Greater,
    GreaterOrEqual,
    Less,
    LessOrEqual
}

public record RequiredVersion(RequiredVersionAction Action, string Name, VersionNumber Version)
{
    // 符合需求
    public bool IsMatch(VersionNumber version)
    {
        if (Name.Length != 0)
        {
            return false;
        }
        return Action switch
        {
            RequiredVersionAction.Equal => version == Version,
            RequiredVersionAction.Greater => version > Version,
            RequiredVersionAction.GreaterOrEqual => version >= Version,
            RequiredVersionAction.Less => version < Version,
            RequiredVersionAction.LessOrEqual => version <= Version,
            _ => false
        };
    }

    public bool IsMatch(FrameworkVersion version)
    {
        if (!string.Equals(Name, version.Name, StringComparison.CurrentCultureIgnoreCase))
        {
            return false;
        }
        return Action switch
        {
            RequiredVersionAction.Equal => version.Version == Version,
            RequiredVersionAction.Greater => version.Version > Version,
            RequiredVersionAction.GreaterOrEqual => version.Version >= Version,
            RequiredVersionAction.Less => version.Version < Version,
            RequiredVersionAction.LessOrEqual => version.Version <= Version,
            _ => false
        };
    }
}

