namespace LanguageServer.Server.Resource;

public class ResourceManager
{
    public ResourceConfig Config { get; set; } = new();

    private char[] TrimChars { get; } = ['\\', '/'];
    
    public string? ResolvePath(string path)
    {
        if (File.Exists(path))
        {
            return path;
        }

        path = path.Trim(TrimChars);

        return Config.Paths.Select(root => Path.Combine(root, path)).FirstOrDefault(File.Exists);
    }

    public IEnumerable<string> GetFiles(string path)
    {
        return Config.Paths
            .Select(root => Path.Combine(root, path))
            .Where(Directory.Exists)
            .SelectMany(directory => Directory.EnumerateFiles(directory, "*", SearchOption.TopDirectoryOnly));
    }
}