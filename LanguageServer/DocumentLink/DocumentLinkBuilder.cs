using EmmyLua.CodeAnalysis.Document;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;
using LanguageServer.Server.Resource;
using LanguageServer.Util;

namespace LanguageServer.DocumentLink;

public class DocumentLinkBuilder
{
    public List<OmniSharp.Extensions.LanguageServer.Protocol.Models.DocumentLink> Build(
        LuaDocument document,
        ResourceManager resourceManager)
    {
        var links = new List<OmniSharp.Extensions.LanguageServer.Protocol.Models.DocumentLink>();
        var stringTokens = document.SyntaxTree.SyntaxRoot.DescendantsWithToken.OfType<LuaStringToken>();
        foreach (var stringToken in stringTokens)
        {
            var path = stringToken.Value;
            if (resourceManager.MayFilePath(path))
            {
                var targetPath = resourceManager.ResolvePath(path);
                if (targetPath is not null)
                {
                    var link = new OmniSharp.Extensions.LanguageServer.Protocol.Models.DocumentLink
                    {
                        Range = stringToken.Range.ToLspRange(document),
                        Target = targetPath
                    };
                    links.Add(link);
                }
            }
        }

        return links;
    }
}