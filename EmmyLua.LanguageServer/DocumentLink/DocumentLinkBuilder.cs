using EmmyLua.CodeAnalysis.Document;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;
using EmmyLua.LanguageServer.Server.Resource;
using EmmyLua.LanguageServer.Util;

namespace EmmyLua.LanguageServer.DocumentLink;

public class DocumentLinkBuilder
{
    public List<Framework.Protocol.Message.DocumentLink.DocumentLink> Build(
        LuaDocument document,
        ResourceManager resourceManager)
    {
        var links = new List<Framework.Protocol.Message.DocumentLink.DocumentLink>();
        var stringTokens = document.SyntaxTree.SyntaxRoot.DescendantsWithToken.OfType<LuaStringToken>();
        foreach (var stringToken in stringTokens)
        {
            var path = stringToken.Value;
            if (resourceManager.MayFilePath(path))
            {
                var targetPath = resourceManager.ResolvePath(path);
                if (targetPath is not null)
                {
                    var link = new Framework.Protocol.Message.DocumentLink.DocumentLink
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