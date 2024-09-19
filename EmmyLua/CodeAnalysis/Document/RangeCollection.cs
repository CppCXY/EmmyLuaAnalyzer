namespace EmmyLua.CodeAnalysis.Document;

public class RangeCollection
{
    private List<SourceRange> Ranges { get; } = new();

    public void AddRange(SourceRange range)
    {
        var left = 0;
        var right = Ranges.Count - 1;

        while (left <= right)
        {
            var mid = left + (right - left) / 2;
            if (Ranges[mid].StartOffset < range.StartOffset)
            {
                left = mid + 1;
            }
            else
            {
                right = mid - 1;
            }
        }

        Ranges.Insert(left, range);
    }

    public bool Contains(int offset)
    {
        var left = 0;
        var right = Ranges.Count - 1;

        while (left <= right)
        {
            var mid = left + (right - left) / 2;
            var range = Ranges[mid];

            if (range.Contain(offset))
            {
                return true;
            }

            if (offset < range.StartOffset)
            {
                right = mid - 1;
            }
            else
            {
                left = mid + 1;
            }
        }

        return false;
    }
}
