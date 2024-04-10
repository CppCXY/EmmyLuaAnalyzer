namespace LanguageServer.Monitor;

public class ProgressReport
{
    public string text;

    public double percent;
}

public class ServerStatusParams
{
    public string health;
    
    public string? message;
    
    public bool? loading;
}