namespace EmmyLua.CodeAnalysis.Common;

public interface ILocation
{
    public IDocument Document { get; }

    public int StartLine { get; }

    public int StartCol { get; }

    public int EndLine { get; }

    public int EndCol { get; }

    public string UriLocation => $"{Document.Uri}#{StartLine}:{StartCol}-{EndLine}:{EndCol}";
}

