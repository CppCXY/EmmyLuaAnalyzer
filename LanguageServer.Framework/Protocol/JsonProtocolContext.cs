using System.Text.Json.Serialization;
using EmmyLua.LanguageServer.Framework.Protocol.JsonRpc;
using EmmyLua.LanguageServer.Framework.Protocol.Model;
using EmmyLua.LanguageServer.Framework.Protocol.Model.Diagnostic;
using EmmyLua.LanguageServer.Framework.Protocol.Model.WorkspaceEdit;
using EmmyLua.LanguageServer.Framework.Protocol.Server.Request.Initialize;
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
[JsonSerializable(typeof(ResponseError))]
[JsonSerializable(typeof(InitializeParams))]
[JsonSerializable(typeof(InitializeResponse))]
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
// ReSharper disable once ClassNeverInstantiated.Global
internal partial class JsonProtocolContext: JsonSerializerContext;
