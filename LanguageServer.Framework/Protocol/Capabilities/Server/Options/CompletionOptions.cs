using System.Text.Json.Serialization;

namespace EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Server.Options;

public class CompletionOptions : WorkDoneProgressOptions
{
    /**
     * Most tools trigger completion request automatically without explicitly
     * requesting it using a keyboard shortcut (e.g., Ctrl+Space). Typically they
     * do so when the user starts to type an identifier. For example, if the user
     * types `c` in a JavaScript file, code complete will automatically pop up and
     * present `console` besides others as a completion item. Characters that
     * make up identifiers don't need to be listed here.
     *
     * If code complete should automatically be triggered on characters not being
     * valid inside an identifier (for example, `.` in JavaScript), list them in
     * `triggerCharacters`.
     */
    [JsonPropertyName("triggerCharacters")]
    public List<string>? TriggerCharacters { get; init; }

    /**
     * The list of all possible characters that commit a completion. This field
     * can be used if clients don't support individual commit characters per
     * completion item. See client capability
     * `completion.completionItem.commitCharactersSupport`.
     *
     * If a server provides both `allCommitCharacters` and commit characters on
     * an individual completion item, the ones on the completion item win.
     *
     * @since 3.2.0
     */
    [JsonPropertyName("allCommitCharacters")]
    public List<string>? AllCommitCharacters { get; init; }

    /**
     * The server provides support to resolve additional
     * information for a completion item.
     */
    [JsonPropertyName("resolveProvider")]
    public bool ResolveProvider { get; init; }

    /**
     * The server supports the following `CompletionItem` specific
     * capabilities.
     *
     * @since 3.17.0
     */
    [JsonPropertyName("completionItem")]
    public CompletionItemDetailOptions? CompletionItem { get; init; }
}

public class CompletionItemDetailOptions
{
    /**
     * The server has support for completion item label
     * details (see also `CompletionItemLabelDetails`) when receiving
     * a completion item in a resolve call.
     *
     * @since 3.17.0
     */
    [JsonPropertyName("labelDetailsSupport")]
    public bool? LabelDetailsSupport { get; init; }
}
