using System.Text;

namespace EmmyLua.CodeAnalysis.Diagnostics;

// ReSharper disable once UnusedType.Global
public class DiagnosticProfile
{
    private Dictionary<DiagnosticCode, long> DiagnosticTime { get; } = new();

    private List<DiagnosticCode> CurrentCodes { get; set; } = new();

    private long CurrentTime { get; set; }

    public void Start(List<DiagnosticCode> codes)
    {
        CurrentCodes = codes;
        CurrentTime = DateTime.Now.Ticks;
    }

    public void Record()
    {
        var time = DateTime.Now.Ticks - CurrentTime;
        foreach (var currentCode in CurrentCodes)
        {
            if (DiagnosticTime.TryGetValue(currentCode, out var oldTime))
            {
                DiagnosticTime[currentCode] = oldTime + time;
            }
            else
            {
                DiagnosticTime[currentCode] = time;
            }
        }
    }

    public string GetProfile()
    {
        var sb = new StringBuilder();
        foreach (var (code, time) in DiagnosticTime)
        {
            sb.AppendLine($"{DiagnosticCodeHelper.GetName(code)}: cost {time / 1e7 } s");
        }

        return sb.ToString();
    }

    public void Reset()
    {
        DiagnosticTime.Clear();
    }
}
