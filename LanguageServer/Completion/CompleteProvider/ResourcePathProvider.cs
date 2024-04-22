using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace LanguageServer.Completion.CompleteProvider;

public class ResourcePathProvider : ICompleteProviderBase
{
    private char[] PathSeparators { get; } = ['\\', '/'];

    public void AddCompletion(CompleteContext context)
    {
        var trigger = context.TriggerToken;
        if (trigger is LuaStringToken stringToken)
        {
            var partialFilePath = stringToken.Value;
            if (context.ServerContext.ResourceManager.MayFilePath(partialFilePath))
            {
                var lastIndex = partialFilePath.LastIndexOfAny(PathSeparators);
                var dir = lastIndex == -1 ? string.Empty : partialFilePath[..(lastIndex + 1)];
                var files = context.ServerContext.ResourceManager.GetFileSystemEntries(dir);
                foreach (var file in files)
                {
                    var fileName = Path.GetFileName(file).Trim(PathSeparators);
                    var filterText = dir + fileName;
                    var kind = File.Exists(file) ? CompletionItemKind.File : CompletionItemKind.Folder;
                    context.Add(new CompletionItem()
                    {
                        Label = fileName,
                        Detail = new Uri(file).AbsoluteUri,
                        Kind = kind,
                        FilterText = filterText,
                        InsertText = filterText
                    });
                }


                context.StopHere();
            }
        }
    }
}