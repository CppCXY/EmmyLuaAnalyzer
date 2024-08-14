using EmmyLua.LanguageServer.Formatting;
using Microsoft.Extensions.FileSystemGlobbing;

namespace EmmyLua.LanguageServer.Server.Editorconfig;

public class EditorconfigWatcher
{
    public void LoadWorkspaceEditorconfig(string workspace)
    {
        var matcher = new Matcher();
        matcher.AddInclude("**/.editorconfig");
        var result = matcher.GetResultsInFullPath(workspace);
        foreach (var editorconfig in result)
        {
            if (Path.GetDirectoryName(editorconfig) is { } directoryName)
            {
                var editorconfigWorkspace = Path.GetFullPath(directoryName);
                FormattingNativeApi.UpdateCodeStyle(editorconfigWorkspace, editorconfig);
            }
        }
    }
}