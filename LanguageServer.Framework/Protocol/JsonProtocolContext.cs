using System.Text.Json.Serialization;
using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Client.TextDocumentClientCapabilities;
using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Client.WorkspaceEditClientCapabilities;
using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Server;
using EmmyLua.LanguageServer.Framework.Protocol.JsonRpc;
using EmmyLua.LanguageServer.Framework.Protocol.Model;
using EmmyLua.LanguageServer.Framework.Protocol.Model.Diagnostic;
using EmmyLua.LanguageServer.Framework.Protocol.Model.File;
using EmmyLua.LanguageServer.Framework.Protocol.Model.Kind;
using EmmyLua.LanguageServer.Framework.Protocol.Model.Union;
using EmmyLua.LanguageServer.Framework.Protocol.Model.WorkDoneProgress;
using EmmyLua.LanguageServer.Framework.Protocol.Notification;
using EmmyLua.LanguageServer.Framework.Protocol.Request.Initialize;
using Range = System.Range;

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
[JsonSerializable(typeof(Message))]
[JsonSerializable(typeof(MethodMessage))]
[JsonSerializable(typeof(RequestMessage))]
[JsonSerializable(typeof(ResponseMessage))]
[JsonSerializable(typeof(NotificationMessage))]
[JsonSerializable(typeof(ResponseError))]
[JsonSerializable(typeof(InitializeParams))]
[JsonSerializable(typeof(InitializeResult))]
[JsonSerializable(typeof(ClientInfo))]
[JsonSerializable(typeof(ClientInfo))]
[JsonSerializable(typeof(DocumentUri))]
[JsonSerializable(typeof(DocumentFilter))]
[JsonSerializable(typeof(Range))]
[JsonSerializable(typeof(Position))]
[JsonSerializable(typeof(Location))]
[JsonSerializable(typeof(LocationLink))]
[JsonSerializable(typeof(TextDocumentIdentifier))]
[JsonSerializable(typeof(TextDocumentItem))]
[JsonSerializable(typeof(VersionedTextDocumentIdentifier))]
[JsonSerializable(typeof(TextDocumentEdit))]
[JsonSerializable(typeof(AnnotatedTextEdit))]
[JsonSerializable(typeof(MarkupContent))]
[JsonSerializable(typeof(Command))]
[JsonSerializable(typeof(SnippetTextEdit))]
[JsonSerializable(typeof(TextEdit))]
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
// ReSharper disable once ClassNeverInstantiated.Global
internal partial class JsonProtocolContext: JsonSerializerContext;
