using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace LanguageServer.Completion.CompleteProvider;

public class ResourcePathProvider : ICompleteProviderBase
{
    public void AddCompletion(CompleteContext context)
    {
        var trigger = context.TriggerToken;
        if (trigger is LuaStringToken stringToken)
        {
            var partialFilePath = stringToken.Value;
            if (context.ServerContext.ResourceManager.MayFilePath(partialFilePath))
            {
                var parts = partialFilePath.Split('/', '\\');
                if (parts.Length > 1)
                {
                    // TODO: Implement this
                    // var dir0 = Path.GetDirectoryName(partialFilePath);
                    // var dir = string.Join('/', parts[..^1]);
                    // var files = context.ServerContext.ResourceManager.GetFiles(dir);
                    // foreach (var file in files)
                    // {
                    //     var fileName = Path.GetFileName(file);
                    //     context.Add(new CompletionItem()
                    //     {
                    //         Label = fileName,
                    //         // Detail = new Uri(file).AbsoluteUri,
                    //         // Kind = CompletionItemKind.File
                    //     });
                    // }
                }

                context.StopHere();
            }
        }
    }
}