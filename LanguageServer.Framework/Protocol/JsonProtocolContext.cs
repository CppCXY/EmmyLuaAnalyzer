using System.Text.Json.Serialization;
using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Client.WorkspaceEditClientCapabilities;
using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Server;
using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Server.Options;
using EmmyLua.LanguageServer.Framework.Protocol.JsonRpc;
using EmmyLua.LanguageServer.Framework.Protocol.Message;
using EmmyLua.LanguageServer.Framework.Protocol.Message.CallHierarchy;
using EmmyLua.LanguageServer.Framework.Protocol.Message.Declaration;
using EmmyLua.LanguageServer.Framework.Protocol.Message.Definition;
using EmmyLua.LanguageServer.Framework.Protocol.Message.Implementation;
using EmmyLua.LanguageServer.Framework.Protocol.Message.Initialize;
using EmmyLua.LanguageServer.Framework.Protocol.Message.NotebookDocument;
using EmmyLua.LanguageServer.Framework.Protocol.Message.Reference;
using EmmyLua.LanguageServer.Framework.Protocol.Message.Registration;
using EmmyLua.LanguageServer.Framework.Protocol.Message.TextDocument;
using EmmyLua.LanguageServer.Framework.Protocol.Message.TypeDefinition;
using EmmyLua.LanguageServer.Framework.Protocol.Model;
using EmmyLua.LanguageServer.Framework.Protocol.Model.Diagnostic;
using EmmyLua.LanguageServer.Framework.Protocol.Model.File;
using EmmyLua.LanguageServer.Framework.Protocol.Model.Kind;
using EmmyLua.LanguageServer.Framework.Protocol.Model.TextDocument;
using EmmyLua.LanguageServer.Framework.Protocol.Model.TextEdit;
using EmmyLua.LanguageServer.Framework.Protocol.Model.Union;
using EmmyLua.LanguageServer.Framework.Protocol.Model.WorkDoneProgress;
using Range = EmmyLua.LanguageServer.Framework.Protocol.Model.Range;


namespace EmmyLua.LanguageServer.Framework.Protocol;

[JsonSourceGenerationOptions(GenerationMode = JsonSourceGenerationMode.Metadata)]
[JsonSerializable(typeof(uint))]
[JsonSerializable(typeof(int))]
[JsonSerializable(typeof(bool))]
[JsonSerializable(typeof(int?))]
[JsonSerializable(typeof(bool?))]
[JsonSerializable(typeof(uint?))]
[JsonSerializable(typeof(string))]
[JsonSerializable(typeof(Uri))]
[JsonSerializable(typeof(JsonRpc.Message))]
[JsonSerializable(typeof(MethodMessage))]
[JsonSerializable(typeof(RequestMessage))]
[JsonSerializable(typeof(ResponseMessage))]
[JsonSerializable(typeof(NotificationMessage))]
[JsonSerializable(typeof(ResponseError))]
[JsonSerializable(typeof(InitializeParams))]
[JsonSerializable(typeof(InitializeResult))]
[JsonSerializable(typeof(ClientInfo))]
[JsonSerializable(typeof(DocumentUri))]
[JsonSerializable(typeof(DocumentFilter))]
[JsonSerializable(typeof(Range))]
[JsonSerializable(typeof(List<Range>))]
[JsonSerializable(typeof(Position))]
[JsonSerializable(typeof(List<Position>))]
[JsonSerializable(typeof(Location))]
[JsonSerializable(typeof(List<Location>))]
[JsonSerializable(typeof(LocationLink))]
[JsonSerializable(typeof(List<LocationLink>))]
[JsonSerializable(typeof(TextDocumentIdentifier))]
[JsonSerializable(typeof(TextDocumentItem))]
[JsonSerializable(typeof(VersionedTextDocumentIdentifier))]
[JsonSerializable(typeof(TextDocumentEdit))]
[JsonSerializable(typeof(AnnotatedTextEdit))]
[JsonSerializable(typeof(MarkupContent))]
[JsonSerializable(typeof(Command))]
[JsonSerializable(typeof(SnippetTextEdit))]
[JsonSerializable(typeof(TextEdit))]
[JsonSerializable(typeof(List<TextEdit>))]
[JsonSerializable(typeof(WorkspaceEdit))]
[JsonSerializable(typeof(TraceValue))]
[JsonSerializable(typeof(ChangeAnnotation))]
[JsonSerializable(typeof(Diagnostic))]
[JsonSerializable(typeof(DiagnosticSeverity))]
[JsonSerializable(typeof(DiagnosticTag))]
[JsonSerializable(typeof(DiagnosticRelatedInformation))]
[JsonSerializable(typeof(CreateFile))]
[JsonSerializable(typeof(CreateFileOptions))]
[JsonSerializable(typeof(RenameFile))]
[JsonSerializable(typeof(RenameFileOptions))]
[JsonSerializable(typeof(DeleteFile))]
[JsonSerializable(typeof(DeleteFileOptions))]
[JsonSerializable(typeof(ChangeAnnotationIdentifier))]
[JsonSerializable(typeof(WorkDoneProgressBegin))]
[JsonSerializable(typeof(WorkDoneProgressReport))]
[JsonSerializable(typeof(WorkDoneProgressEnd))]
[JsonSerializable(typeof(CodeActionKind))]
[JsonSerializable(typeof(CompletionItemKind))]
[JsonSerializable(typeof(FoldingRangeKind))]
[JsonSerializable(typeof(InsertTextMode))]
[JsonSerializable(typeof(MessageType))]
[JsonSerializable(typeof(PositionEncodingKind))]
[JsonSerializable(typeof(ResourceOperationKind))]
[JsonSerializable(typeof(PrepareSupportDefaultBehavior))]
[JsonSerializable(typeof(SymbolKind))]
[JsonSerializable(typeof(SymbolTag))]
[JsonSerializable(typeof(TokenFormat))]
[JsonSerializable(typeof(StringOrInt))]
[JsonSerializable(typeof(StringOrMarkupContent))]
[JsonSerializable(typeof(WorkspaceEditDocumentChanges))]
[JsonSerializable(typeof(ServerCapabilities))]
[JsonSerializable(typeof(InitializeResult))]
[JsonSerializable(typeof(InitializedParams))]
[JsonSerializable(typeof(RegistrationParams))]
[JsonSerializable(typeof(UnregistrationParams))]
[JsonSerializable(typeof(CancelParams))]
[JsonSerializable(typeof(TextDocumentSyncOptions))]
[JsonSerializable(typeof(DidOpenTextDocumentParams))]
[JsonSerializable(typeof(DidChangeTextDocumentParams))]
[JsonSerializable(typeof(DidCloseTextDocumentParams))]
[JsonSerializable(typeof(DidSaveTextDocumentParams))]
[JsonSerializable(typeof(WillSaveTextDocumentParams))]
[JsonSerializable(typeof(DidOpenNotebookDocumentParams))]
[JsonSerializable(typeof(DidChangeNotebookDocumentParams))]
[JsonSerializable(typeof(DidCloseNotebookDocumentParams))]
[JsonSerializable(typeof(DidSaveNotebookDocumentParams))]
[JsonSerializable(typeof(DeclarationParams))]
[JsonSerializable(typeof(DeclarationResponse))]
[JsonSerializable(typeof(DefinitionParams))]
[JsonSerializable(typeof(DefinitionResponse))]
[JsonSerializable(typeof(TypeDefinitionParams))]
[JsonSerializable(typeof(TypeDefinitionResponse))]
[JsonSerializable(typeof(ImplementationParams))]
[JsonSerializable(typeof(ImplementationResponse))]
[JsonSerializable(typeof(ReferenceParams))]
[JsonSerializable(typeof(ReferenceResponse))]
[JsonSerializable(typeof(CallHierarchyItem))]
[JsonSerializable(typeof(List<CallHierarchyItem>))]
[JsonSerializable(typeof(CallHierarchyPrepareParams))]
[JsonSerializable(typeof(CallHierarchyPrepareResponse))]
[JsonSerializable(typeof(CallHierarchyIncomingCallsParams))]
[JsonSerializable(typeof(CallHierarchyIncomingCallsResponse))]
[JsonSerializable(typeof(CallHierarchyIncomingCall))]
[JsonSerializable(typeof(CallHierarchyOutgoingCallsParams))]
[JsonSerializable(typeof(CallHierarchyOutgoingCallsResponse))]
[JsonSerializable(typeof(CallHierarchyOutgoingCall))]
// ReSharper disable once ClassNeverInstantiated.Global
internal partial class JsonProtocolContext: JsonSerializerContext;
