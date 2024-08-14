using EmmyLua.CodeAnalysis.Document;
using EmmyLua.LanguageServer.Formatting;
using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Client.ClientCapabilities;
using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Server;
using EmmyLua.LanguageServer.Framework.Protocol.Message.Client.Registration;
using EmmyLua.LanguageServer.Framework.Protocol.Message.WorkspaceWatchedFile;
using EmmyLua.LanguageServer.Framework.Protocol.Message.WorkspaceWatchedFile.Watch;
using EmmyLua.LanguageServer.Framework.Server.Handler;
using EmmyLua.LanguageServer.Server;

namespace EmmyLua.LanguageServer.TextDocument;

// ReSharper disable once ClassNeverInstantiated.Global
public class DidChangeWatchedFilesHandler(ServerContext context)
    : DidChangeWatchedFilesHandlerBase
{
    private Task UpdateOneFileEventAsync(FileEvent fileEvent, CancellationToken cancellationToken)
    {
        switch (fileEvent.Type)
        {
            case FileChangeType.Created:
            case FileChangeType.Changed:
            {
                try
                {
                    var fileSystemPath = fileEvent.Uri.FileSystemPath;
                    if (IsEditorConfigFile(fileSystemPath))
                    {
                        UpdateEditorConfigFile(fileSystemPath);
                        return Task.CompletedTask;
                    }
                    
                    if (context.LuaProject.IsExclude(fileSystemPath))
                    {
                        return Task.CompletedTask;
                    }
                    
                    var fileText = context.LuaProject.ReadFile(fileEvent.Uri.FileSystemPath);
                    var uri = fileEvent.Uri.UnescapeUri;
                    context.UpdateDocument(uri, fileText, cancellationToken);
                }
                catch (Exception e)
                {
                    Console.Error.WriteLine(e.Message);
                }

                break;
            }
            case FileChangeType.Deleted:
            {
                var uri = fileEvent.Uri.UnescapeUri;
                context.RemoveDocument(uri);
                break;
            }
        }

        return Task.CompletedTask;
    }

    private Task UpdateManyFileEventAsync(List<FileEvent> fileEvents,
        CancellationToken cancellationToken)
    {
        var documentIds = new List<LuaDocumentId>();
        context.ReadyWrite(() =>
        {
            context.LuaProject.Compilation.BulkUpdate(() =>
            {
                foreach (var fileEvent in fileEvents)
                {
                    switch (fileEvent)
                    {
                        case { Type: FileChangeType.Created }:
                        case { Type: FileChangeType.Changed }:
                        {
                            var filePath = fileEvent.Uri.FileSystemPath;
                            if (IsEditorConfigFile(filePath))
                            {
                                UpdateEditorConfigFile(filePath);
                                continue;
                            }
                            
                            if (context.LuaProject.IsExclude(filePath))
                            {
                                continue;
                            }

                            var uri = fileEvent.Uri.UnescapeUri;
                            var fileText = File.ReadAllText(filePath);
                            context.LuaProject.UpdateDocumentByUri(uri, fileText);
                            var documentId = context.LuaProject.GetDocumentIdByUri(uri);
                            if (documentId.HasValue)
                            {
                                documentIds.Add(documentId.Value);
                            }

                            break;
                        }
                        case { Type: FileChangeType.Deleted }:
                        {
                            context.LuaProject.RemoveDocumentByUri(fileEvent.Uri.UnescapeUri);
                            break;
                        }
                    }
                }
            });
        });

        foreach (var documentId in documentIds)
        {
            context.PushDocumentDiagnostics(documentId);
        }
        return Task.CompletedTask;
    }

    protected override async Task Handle(DidChangeWatchedFilesParams request, CancellationToken token)
    {
        var changes = request.Changes.ToList();
        if (changes.Count == 1)
        {
            await UpdateOneFileEventAsync(changes[0], token);
        }
        else
        {
            await UpdateManyFileEventAsync(changes, token);
        }
    }

    public override void RegisterCapability(ServerCapabilities serverCapabilities,
        ClientCapabilities clientCapabilities)
    {
    }

    public override void RegisterDynamicCapability(Framework.Server.LanguageServer server,
        ClientCapabilities clientCapabilities)
    {
        var dynamicRegistration = new DidChangeWatchedFilesRegistrationOptions()
        {
            Watchers =
            [
                new()
                {
                    GlobalPattern = "**/*.lua",
                    Kind = WatchKind.Create | WatchKind.Change | WatchKind.Delete
                },
                new ()
                {
                    GlobalPattern = "**/.editorconfig",
                    Kind = WatchKind.Create | WatchKind.Change | WatchKind.Delete
                }
            ]
        };

        server.Client.DynamicRegisterCapability(new RegistrationParams()
        {
            Registrations =
            [
                new Registration()
                {
                    Id = Guid.NewGuid().ToString(),
                    Method = "workspace/didChangeWatchedFiles",
                    RegisterOptions = dynamicRegistration
                }
            ]
        });
    }
    
    private bool IsEditorConfigFile(string fileSystemPath)
    {
        return Path.GetFileName(fileSystemPath) == ".editorconfig";
    }
    
    private void UpdateEditorConfigFile(string fileSystemPath)
    {
        var directory = Path.GetDirectoryName(fileSystemPath);
        if (directory is not null)
        {
            var workspace = Path.GetFullPath(directory);
            FormattingNativeApi.UpdateCodeStyle(workspace, fileSystemPath);
        }
    }
}