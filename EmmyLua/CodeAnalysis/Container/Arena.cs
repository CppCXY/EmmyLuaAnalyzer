namespace EmmyLua.CodeAnalysis.Container;

public record struct ArenaId(int Id);
// TODO: This is a simple implementation of Arena<TValue> that is not used in the project.
class Arena<TValue>
{
    private List<TValue> Values { get; } = new();

    private Queue<int> FreeList { get; } = new();

    public ArenaId Add(TValue value)
    {
        var id = GetFreeId();
        if (id.Id < Values.Count)
        {
            Values[id.Id] = value;
        }
        else
        {
            Values.Add(value);
        }

        return id;
    }

    private void Clear(ArenaId id)
    {
        if (id.Id < Values.Count)
        {
            Values[id.Id] = default;
            FreeList.Enqueue(id.Id);
        }
    }

    public TValue? Get(ArenaId id) => id.Id < Values.Count ? Values[id.Id] : default;

    private ArenaId GetFreeId()
    {
        if (FreeList.Count > 0)
        {
            return new ArenaId(FreeList.Dequeue());
        }
        else
        {
            return new ArenaId(Values.Count);
        }
    }
}
