using System.Text.Json.Serialization;
using EmmyLua.LanguageServer.Framework.Protocol.Model;
using EmmyLua.LanguageServer.Framework.Protocol.Model.Kind;

namespace EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Server.Options;

public class CodeActionOptions : WorkDoneProgressOptions
{
    /**
     * CodeActionKinds that this server may return.
     *
     * The list of kinds may be generic, such as `CodeActionKind.Refactor`,
     * or the server may list out every specific kind they provide.
     */
    [JsonPropertyName("codeActionKinds")]
    public List<CodeActionKind> CodeActionKinds { get; set; }

    /**
     * Static documentation for a class of code actions.
     *
     * Documentation from the provider should be shown in the code actions
     * menu if either:
     *
     * - Code actions of `kind` are requested by the editor. In this case,
     *   the editor will show the documentation that most closely matches the
     *   requested code action kind. For example, if a provider has
     *   documentation for both `Refactor` and `RefactorExtract`, when the
     *   user requests code actions for `RefactorExtract`, the editor will use
     *   the documentation for `RefactorExtract` instead of the documentation
     *   for `Refactor`.
     *
     * - Any code actions of `kind` are returned by the provider.
     *
     * At most one documentation entry should be shown per provider.
     *
     * @since 3.18.0
     * @proposed
     */
    [JsonPropertyName("documentation")]
    public List<CodeActionKindDocumentation> Documentation { get; set; }
}

/**
 * Documentation for a class of code actions.
 *
 * @since 3.18.0
 * @proposed
 */
public class CodeActionKindDocumentation
{
    /**
     * The kind of the code action being documented.
     *
     * If the kind is generic, such as `CodeActionKind.Refactor`, the
     * documentation will be shown whenever any refactorings are returned. If
     * the kind is more specific, such as `CodeActionKind.RefactorExtract`, the
     * documentation will only be shown when extract refactoring code actions
     * are returned.
     */
    [JsonPropertyName("kind")]
    public CodeActionKind Kind { get; set; }

    /**
     * Command that is used to display the documentation to the user.
     *
     * The title of this documentation code action is taken
     * from {@linkcode Command.title}
     */
    [JsonPropertyName("command")]
    public Command Command { get; set; } = null!;

    /**
     * The server provides support to resolve additional
     * information for a code action.
     *
     * @since 3.16.0
     */
    [JsonPropertyName("resolveProvider")]
    public bool? ResolveProvider { get; set; }
}
