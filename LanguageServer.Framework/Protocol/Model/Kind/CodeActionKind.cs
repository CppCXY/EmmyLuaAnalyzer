using System.Text.Json;
using System.Text.Json.Serialization;

namespace EmmyLua.LanguageServer.Framework.Protocol.Model.Kind;

[JsonConverter(typeof(CodeActionKindJsonConverter))]
public readonly record struct CodeActionKind(string Value)
{
    /**
     * Base kind for quickfix actions: 'quickfix'.
     */
    public static readonly CodeActionKind QuickFix = new("quickfix");

    /**
     * Base kind for refactoring actions: 'refactor'.
     */
    public static readonly CodeActionKind Refactor = new("refactor");

    /**
     * Base kind for refactoring extraction actions: 'refactor.extract'.
     *
     * Example extract actions:
     *
     * - Extract method
     * - Extract function
     * - Extract variable
     * - Extract interface from class
     * - ...
     */
    public static readonly CodeActionKind RefactorExtract = new("refactor.extract");

    /**
     * Base kind for refactoring inline actions: 'refactor.inline'.
     *
     * Example inline actions:
     *
     * - Inline function
     * - Inline variable
     * - Inline constant
     * - ...
     */
    public static readonly CodeActionKind RefactorInline = new("refactor.inline");

    /**
     * Base kind for refactoring move actions: 'refactor.move'
     *
     * Example move actions:
     *
     * - Move a function to a new file
     * - Move a property between classes
     * - Move method to base class
     * - ...
     *
     * @since 3.18.0 - proposed
     */
    public static readonly CodeActionKind RefactorMove = new("refactor.move");

    /**
     * Base kind for refactoring rewrite actions: 'refactor.rewrite'.
     *
     * Example rewrite actions:
     *
     * - Convert JavaScript function to class
     * - Add or remove parameter
     * - Encapsulate field
     * - Make method static
     * - ...
     */
    public static readonly CodeActionKind RefactorRewrite = new("refactor.rewrite");

    /**
     * Base kind for source actions: `source`.
     *
     * Source code actions apply to the entire file.
     */
    public static readonly CodeActionKind Source = new("source");

    /**
     * Base kind for an organize imports source action:
     * `source.organizeImports`.
     */
    public static readonly CodeActionKind SourceOrganizeImports = new("source.organizeImports");

    /**
     * Base kind for a 'fix all' source action: `source.fixAll`.
     *
     * 'Fix all' actions automatically fix errors that have a clear fix that
     * do not require user input. They should not suppress errors or perform
     * unsafe fixes such as generating new types or classes.
     *
     * @since 3.17.0
     */
    public static readonly CodeActionKind SourceFixAll = new("source.fixAll");

    /**
     * Base kind for all code actions applying to the entire notebook's scope. CodeActionKinds using
     * this should always begin with `notebook.`
     *
     * @since 3.18.0
     */
    public static readonly CodeActionKind Notebook = new("notebook");

    public string Value { get; } = Value;
}

public class CodeActionKindJsonConverter : JsonConverter<CodeActionKind>
{
    public override CodeActionKind Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.String)
        {
            throw new JsonException();
        }

        return new CodeActionKind(reader.GetString() ?? string.Empty);
    }

    public override void Write(Utf8JsonWriter writer, CodeActionKind value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.Value);
    }
}
