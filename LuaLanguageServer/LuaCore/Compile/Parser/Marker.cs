using System.Diagnostics;
using LuaLanguageServer.LuaCore.Kind;

namespace LuaLanguageServer.LuaCore.Compile.Parser;

public record MarkEvent
{
    public sealed record NodeStart(int Parent, LuaSyntaxKind Kind) : MarkEvent;

    public sealed record EatToken(int Index, LuaTokenKind Kind) : MarkEvent;

    public sealed record Error(string Err) : MarkEvent;

    public sealed record NodeEnd : MarkEvent;
}

public interface IMarkerEventContainer
{
    public List<MarkEvent> Events { get; }

    public Marker Marker();
}

public struct Marker
{
    public int Position { get; set; }

    public Marker(int position)
    {
        Position = position;
    }

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
}

public struct CompleteMarker
{
    private int Start { get; }
    private int Finish { get; }
    public LuaSyntaxKind Kind { get; }

    public bool IsComplete { get; }

    public CompleteMarker(int start, int finish, LuaSyntaxKind kind, bool isComplete)
    {
        Start = start;
        Finish = finish;
        Kind = kind;
        IsComplete = isComplete;
    }

    public Marker Precede(IMarkerEventContainer p)
    {
        var m = p.Marker();
        if (p.Events[Start] is MarkEvent.NodeStart(_, _) start)
        {
            p.Events[Start] = start with { Parent = m.Position };
        }

        return m;
    }

    public void Reset(IMarkerEventContainer p, LuaSyntaxKind kind)
    {
        if (p.Events[Start] is MarkEvent.NodeStart(_, _) start)
        {
            p.Events[Start] = start with { Kind = kind };
        }
    }
}
