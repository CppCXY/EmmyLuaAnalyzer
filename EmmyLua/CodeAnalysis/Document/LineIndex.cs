﻿namespace EmmyLua.CodeAnalysis.Document;

public class LineIndex
{
    public static LineIndex Parse(string text)
    {
        var lineIndex = new LineIndex();
        var lineOffset = new LineOffset()
        {
            StartOffset = 0,
            Length = 0,
            ExistSurrogate = false,
        };
        lineIndex._indexs.Add(lineOffset);

        for (var pos = 0; pos < text.Length; pos++)
        {
            var ch = text[pos];

            lineOffset.Length++;
            if (char.IsSurrogate(ch))
            {
                lineOffset.Length++;
                lineOffset.ExistSurrogate = true;
                pos++;
            }
            else if (ch is '\r' or '\n')
            {
                if (ch is '\r' && pos + 1 < text.Length && text[pos + 1] is '\n')
                {
                    pos++;
                    lineOffset.Length++;
                }

                if (pos + 1 >= text.Length) continue;
                lineOffset = new LineOffset()
                {
                    StartOffset = pos + 1,
                    Length = 0,
                    ExistSurrogate = false,
                };
                lineIndex._indexs.Add(lineOffset);
            }
        }

        return lineIndex;
    }

    struct LineOffset
    {
        public int StartOffset { get; set; }
        public int Length { get; set; }
        public bool ExistSurrogate { get; set; }
    }

    private List<LineOffset> _indexs = new();

    public int GetLine(int offset)
    {
        var index = _indexs.BinarySearch(new LineOffset()
        {
            StartOffset = offset,
        }, Comparer<LineOffset>.Create((a, b) => a.StartOffset.CompareTo(b.StartOffset)));

        if (index < 0)
        {
            index = ~index - 1;
        }

        return index;
    }

    public int GetCol(int offset, string source)
    {
        if (offset > source.Length)
        {
            offset = source.Length;
        }
        if (offset < 0)
        {
            offset = 0;
        }

        var line = GetLine(offset);
        var lineOffset = _indexs[line];
        var colOffset = offset - lineOffset.StartOffset;
        var col = 0;
        if (lineOffset.ExistSurrogate)
        {
            for (var pos = lineOffset.StartOffset; pos <= offset; pos++)
            {
                col++;
                if (char.IsSurrogate(source[pos]))
                {
                    pos++;
                }
            }
        }
        else
        {
            col = colOffset;
        }

        return col;
    }

    public int GetOffset(int line, int col, string source)
    {
        if (line >= _indexs.Count)
        {
            return source.Length;
        }
        if (line < 0)
        {
            line = 0;
        }

        var lineOffset = _indexs[line];
        var offset = lineOffset.StartOffset;
        if (lineOffset.ExistSurrogate)
        {
            var colOffset = 0;
            for (var pos = lineOffset.StartOffset; pos < source.Length; pos++)
            {
                if (colOffset == col)
                {
                    offset = pos;
                    break;
                }

                colOffset++;
                if (char.IsSurrogate(source[pos]))
                {
                    pos++;
                }
            }
        }
        else
        {
            offset += col;
        }

        return offset;
    }

    public int TotalLine => _indexs.Count;
}
