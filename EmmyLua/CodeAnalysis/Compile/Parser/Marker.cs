using EmmyLua.CodeAnalysis.Document;
using EmmyLua.CodeAnalysis.Kind;

namespace EmmyLua.CodeAnalysis.Compile.Parser;

public record MarkEvent
{
    public sealed record NodeStart(int Parent, LuaSyntaxKind Kind) : MarkEvent;

    public sealed record EatToken(SourceRange Range, LuaTokenKind Kind) : MarkEvent;

    public sealed record Error(string Err) : MarkEvent;

    public sealed record NodeEnd : MarkEvent;
}

public interface IMarkerEventContainer
{
    public List<MarkEvent> Events { get; }

    public Marker Marker();
}

public struct Marker(int position)
{
    public int Position { get; set; } = position;

    public CompleteMarker Complete(IMarkerEventContainer p, LuaSyntaxKind kind)
    {
        if (p.Events[Position] is MarkEvent.NodeStart(_, _) start)
        {
            p.Events[Position] = start with { Kind = kind };
        }

        var finish = p.Events.Count;
        p.Events.Add(new MarkEvent.NodeEnd());

        return new CompleteMarker(Position, finish, kind, true);
    }

    public CompleteMarker Fail(IMarkerEventContainer p, LuaSyntaxKind kind, string err)
    {
        if (p.Events[Position] is MarkEvent.NodeStart(_, _) start)
        {
            p.Events[Position] = start with { Kind = kind };
        }

        var finish = p.Events.Count;
        p.Events.Add(new MarkEvent.Error(err));
        p.Events.Add(new MarkEvent.NodeEnd());

        return new CompleteMarker(Position, finish, kind, false);
    }

    public bool IsInvalid(IMarkerEventContainer p)
    {
        return (p.Events.Count - 1) == position;
    }
}

public struct CompleteMarker(int start, int finish, LuaSyntaxKind kind, bool isComplete)
{
    public static CompleteMarker Empty { get; } = new(-1, -1, LuaSyntaxKind.None, false);
    private int Start { get; } = start;
    private int Finish { get; } = finish;
    public LuaSyntaxKind Kind { get; } = kind;

    public bool IsComplete { get; } = isComplete;

    public Marker Precede(IMarkerEventContainer p)
    {
        var m = p.Marker();
        if (p.Events[Start] is MarkEvent.NodeStart(_, _) start)
        {
            p.Events[Start] = start with { Parent = m.Position };
        }

        return m;
    }

    public Marker Reset(IMarkerEventContainer p)
    {
        if (p.Events[Start] is MarkEvent.NodeStart(_, _) start)
        {
            p.Events[Start] = start with { Kind = LuaSyntaxKind.None };
        }

        p.Events.RemoveAt(Finish);
        return new Marker(Start);
    }
}
