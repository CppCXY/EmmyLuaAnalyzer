using EmmyLua.CodeAnalysis.Document;

namespace EmmyLua.CodeAnalysis.Syntax.Tree.Red;

public record struct RedNode(
    int RawKind,
    SourceRange Range,
    int Parent,
    int ChildStart,
    int ChildEnd
);
