using System.Text.Json;
using System.Text.Json.Serialization;

namespace EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Client.WorkspaceEditClientCapabilities;

[JsonConverter(typeof(FailureHandlingKindConverter))]
public readonly record struct FailureHandlingKind(string Value)
{
    /**
     * Applying the workspace change is simply aborted if one of the changes
     * provided fails. All operations executed before the failing operation
     * stay executed.
     */
    public static FailureHandlingKind Abort = new("abort");
    
    /**
     * All operations are executed transactionally. That means they either all
     * succeed or no changes at all are applied to the workspace.
     */
    public static FailureHandlingKind Transactional = new("transactional");
    
    /**
     * If the workspace edit contains only textual file changes they are
     * executed transactionally. If resource changes (create, rename or delete
     * file) are part of the change the failure handling strategy is abort.
     */
    public static FailureHandlingKind TextOnlyTransactional = new("textOnlyTransactional");
    
    /**
     * The client tries to undo the operations already executed. But there is no
     * guarantee that this is succeeding.
     */
    public static FailureHandlingKind Undo = new("undo");
    
    public string Value { get; } = Value;
}

public class FailureHandlingKindConverter : JsonConverter<FailureHandlingKind>
{
    public override FailureHandlingKind Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return new FailureHandlingKind(reader.GetString()!);
    }

    public override void Write(Utf8JsonWriter writer, FailureHandlingKind value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.Value);
    }
}