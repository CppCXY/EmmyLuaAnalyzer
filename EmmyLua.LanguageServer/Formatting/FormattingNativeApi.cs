using System.Reflection;
using System.Runtime.InteropServices;

namespace EmmyLua.LanguageServer.Formatting;

public static class FormattingNativeApi
{
    [DllImport("code_format_csharp", CallingConvention = CallingConvention.Cdecl,
        CharSet = CharSet.Ansi)]
    public static extern IntPtr ReformatLuaCode(string code, string uri);

    [StructLayout(LayoutKind.Sequential)]
    public struct RangeFormatResult
    {
        public int StartLine;
        public int StartChar;
        public int EndLine;
        public int EndChar;
        public IntPtr Code;
    }

    [DllImport("code_format_csharp", CallingConvention = CallingConvention.Cdecl,
        CharSet = CharSet.Ansi)]
    public static extern RangeFormatResult RangeFormatLuaCode(string code, string uri, int startLine, int startChar,
        int endLine,
        int endChar);

    [DllImport("code_format_csharp", CallingConvention = CallingConvention.Cdecl)]
    public static extern void FreeReformatResult(IntPtr ptr);

    [DllImport("code_format_csharp", CallingConvention = CallingConvention.Cdecl)]
    public static extern void UpdateCodeStyle(string workspace, string configPath);

    [DllImport("code_format_csharp", CallingConvention = CallingConvention.Cdecl)]
    public static extern void RemoveCodeStyle(string workspace);

    public static void InitNativeApi()
    {
        NativeLibrary.SetDllImportResolver(Assembly.GetExecutingAssembly(), DllImportResolver);
    }

    private static IntPtr DllImportResolver(string libraryName, Assembly assembly, DllImportSearchPath? searchPath)
    {
        if (libraryName == "code_format_csharp")
        {
            var dllPath = GetDllPath();
            if (NativeLibrary.TryLoad(dllPath, out var handle))
            {
                return handle;
            }

            Console.Error.WriteLine("Failed to load native library: " + dllPath);
        }

        // Otherwise, fallback to default import resolver.
        return IntPtr.Zero;
    }

    private static string GetDllPath()
    {
        var basePath = "Formatting/Dll";
        var osFolder = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "Win" :
            RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? "Linux" :
            RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? "Mac" : string.Empty;

        var archFolder = RuntimeInformation.OSArchitecture == Architecture.X64 ? "x64" :
            RuntimeInformation.OSArchitecture == Architecture.Arm64 ? "arm64" : string.Empty;

        return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, basePath, osFolder, archFolder,
            "code_format_csharp");
    }
}