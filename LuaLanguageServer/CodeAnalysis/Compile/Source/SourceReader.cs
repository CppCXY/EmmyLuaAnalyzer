namespace LuaLanguageServer.CodeAnalysis.Compile.Source;

public class SourceReader
{
    public const char Eof = '\0';

    private string Text { get; }
    private SourceRange ValidRange { get; set; }
    private bool IsSavedText { get; set; }
    private int StartPosition { get; set; }
    private int FinishPosition { get; set; }
    public int CurrentPosition { get; set; }

    private SourceReader(string text, SourceRange range)
    {
        Text = text;
        ValidRange = range;
        IsSavedText = false;
        StartPosition = 0;
        CurrentPosition = 0;
        FinishPosition = 0;
        IsEof = false;
    }

    public SourceReader(string text)
        : this(text, new SourceRange(0, text.Length))
    {
    }

    public void Bump()
    {
        Save();
        if (CurrentPosition + 1 < ValidRange.Length)
        {
            ++CurrentPosition;
        }
        else
        {
            IsEof = true;
        }
    }

    public void ResetBuff()
    {
        IsSavedText = false;
        StartPosition = 0;
        FinishPosition = 0;
    }

    public void Reset(SourceRange range)
    {
        ValidRange = range;
        CurrentPosition = 0;
        IsEof = false;
        ResetBuff();
    }

    private void Save()
    {
        if (!IsSavedText)
        {
            IsSavedText = true;
            StartPosition = CurrentPosition;
        }

        FinishPosition = CurrentPosition;
    }

    public char CurrentChar
    {
        get
        {
            if (!IsEof && CurrentPosition < ValidRange.Length)
            {
                return Text[ValidRange.StartOffset + CurrentPosition];
            }

            return Eof;
        }
    }

    public char NextChar
    {
        get
        {
            if (!IsEof && CurrentPosition + 1 < ValidRange.Length)
            {
                return Text[ValidRange.StartOffset + CurrentPosition + 1];
            }

            return Eof;
        }
    }

    public SourceRange SavedRange =>
        new SourceRange(ValidRange.StartOffset + StartPosition, FinishPosition - StartPosition + 1);

    public ReadOnlySpan<char> CurrentSavedText => IsSavedText
        ? Text.AsSpan(ValidRange.StartOffset + StartPosition, FinishPosition - StartPosition + 1)
        : Text.AsSpan(ValidRange.StartOffset + StartPosition, 0);

    public bool HasSavedText => IsSavedText;

    public bool IsEof { get; set; }

    public int EatWhen(char ch)
    {
        var count = 0;
        while (!IsEof && CurrentChar == ch)
        {
            ++count;
            Bump();
        }

        return count;
    }

    public int EatWhen(Func<char, bool> func)
    {
        var count = 0;
        while (!IsEof && func(CurrentChar))
        {
            ++count;
            Bump();
        }

        return count;
    }
}
