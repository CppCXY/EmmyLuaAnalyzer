namespace LuaLanguageServer.LuaCore.Compile.Source;

public class SourceReader
{
    public const char Eof = '\0';

    private string Text { get; }
    private bool IsSavedText { get; set; }
    private int StartPosition { get; set; }
    private int FinishPosition { get; set; }
    private int CurrentPosition { get; set; }

    public SourceReader(string text)
    {
        Text = text;
    }

    public void Bump()
    {
        Save();
        if (CurrentPosition < Text.Length)
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
            if (!IsEof && CurrentPosition < Text.Length)
            {
                return Text[CurrentPosition];
            }

            return Eof;
        }
    }

    public bool CheckNext1(char ch)
    {
        if (CurrentPosition >= Text.Length || Text[CurrentPosition] != ch) return false;
        Bump();
        return true;
    }

    public bool CheckNext2(char ch1, char ch2)
    {
        if (CurrentPosition >= Text.Length || (Text[CurrentPosition] != ch1 && Text[CurrentPosition] != ch2))
            return false;
        Bump();
        return true;
    }

    public SourceRange SavedRange => new SourceRange(StartPosition, FinishPosition - StartPosition + 1);

    public ReadOnlySpan<char> CurrentSavedText => IsSavedText
        ? Text.AsSpan(StartPosition, FinishPosition - StartPosition + 1)
        : Text.AsSpan(StartPosition, 0);

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
