using System.Runtime.InteropServices;

namespace EmmyLua.LanguageServer.Formatting;

public class FormattingBuilder
{
    private bool CanUseNative { get; set; } = true;

    public FormattingBuilder()
    {
        FormattingNativeApi.InitNativeApi();
    }

    public string Format(string code, string filePath)
    {
        if (!CanUseNative)
        {
            return string.Empty;
        }

        var ptr = IntPtr.Zero;
        try
        {
            ptr = FormattingNativeApi.ReformatLuaCode(code, filePath);
            if (ptr == IntPtr.Zero)
            {
                return string.Empty;
            }

            var result = Marshal.PtrToStringAnsi(ptr);

            if (string.IsNullOrEmpty(result))
            {
                return code;
            }

            return result;
        }
        catch (Exception e)
        {
            Console.Error.WriteLine(e.Message);
            CanUseNative = false;
            return string.Empty;
        }
        finally
        {
            if (ptr != IntPtr.Zero)
            {
                FormattingNativeApi.FreeReformatResult(ptr);
            }
        }
    }

    public string RangeFormat(string code, string filePath, ref int startLine, ref int startChar, ref int endLine,
        ref int endChar)
    {
        if (!CanUseNative)
        {
            return string.Empty;
        }

        var ptr = IntPtr.Zero;
        try
        {
            var rangeFormatResult =
                FormattingNativeApi.RangeFormatLuaCode(code, filePath, startLine, startChar, endLine, endChar);
            ptr = rangeFormatResult.Code;
            if (ptr == IntPtr.Zero)
            {
                return string.Empty;
            }

            var result = Marshal.PtrToStringAnsi(ptr);
            startLine = rangeFormatResult.StartLine;
            startChar = rangeFormatResult.StartChar;
            endLine = rangeFormatResult.EndLine;
            endChar = rangeFormatResult.EndChar;

            if (string.IsNullOrEmpty(result))
            {
                return string.Empty;
            }

            return result;
        }
        catch (Exception e)
        {
            Console.Error.WriteLine(e.Message);
            CanUseNative = false;
            return string.Empty;
        }
        finally
        {
            if (ptr != IntPtr.Zero)
            {
                FormattingNativeApi.FreeReformatResult(ptr);
            }
        }
    }
}