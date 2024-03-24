using EmmyLua.CodeAnalysis.Workspace;
using LanguageServer.ExtensionUtil;
using OmniSharp.Extensions.LanguageServer.Protocol;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace LanguageServer.Rename;

// ReSharper disable once ClassNeverInstantiated.Global
public class RenameHandler(LuaWorkspace workspace) : RenameHandlerBase
{
    protected override RenameRegistrationOptions CreateRegistrationOptions(RenameCapability capability,
        ClientCapabilities clientCapabilities)
    {
        return new RenameRegistrationOptions()
        {
            DocumentSelector = new TextDocumentSelector
            (
                new TextDocumentFilter()
                {
                    Pattern = "**/*.lua"
                }
            ),
            PrepareProvider = false
        };
    }

    public override Task<WorkspaceEdit?> Handle(RenameParams request, CancellationToken cancellationToken)
    {
        var uri = request.TextDocument.Uri.ToUnencodedString();
        var semanticModel = workspace.Compilation.GetSemanticModel(uri);
        if (semanticModel is not null)
        {
            var document = semanticModel.Document;
            var pos = request.Position;
            var node = document.SyntaxTree.SyntaxRoot.NodeAt(pos.Line, pos.Character);
            if (node is not null)
            {
                var newName = request.NewName;
                var references = semanticModel.FindReferences(node);
                var edits = references.Select(it => it.ToTextEdit(newName));
                var dic = new Dictionary<DocumentUri, List<TextEdit>>();
                foreach (var edit in edits)
                {
                    if (!dic.TryGetValue(edit.Item1, out var list))
                    {
                        list = new List<TextEdit>();
                        dic[edit.Item1] = list;
                    }
                    list.Add(edit.Item2);
                }

                var changes = new Dictionary<DocumentUri, IEnumerable<TextEdit>>();
                foreach (var it in dic)
                {
                    changes[it.Key] = it.Value;
                }
                
                return Task.FromResult<WorkspaceEdit?>(new WorkspaceEdit()
                {
                    Changes = changes
                });
            }
        }

        return Task.FromResult<WorkspaceEdit?>(null);
    }
}