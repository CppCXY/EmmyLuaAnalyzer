namespace LuaLanguageServer.LuaCore.Kind;

public enum LuaSyntaxKind
{
    None,
    Source,
    Block,

    // statements
    EmptyStat,
    LocalStat,
    LocalFuncStat,
    IfStat,
    IfClauseStat,
    WhileStat,
    DoStat,
    ForStat,
    ForRangeStat,
    RepeatStat,
    FuncStat,
    LabelStat,
    BreakStat,
    ReturnStat,
    GotoStat,
    ExprStat,
    AssignStat,
    UnknownStat,

    // expressions
    SuffixExpr,
    ParenExpr,
    LiteralExpr,
    ClosureExpr,
    UnaryExpr,
    BinaryExpr,
    TableExpr,
    CallExpr,
    IndexExpr,
    NameExpr,

    VarDef,
    TableFieldAssign,
    TableFieldValue,
    Attribute,

    // comment
    Comment,

    // doc
    DocClass,
    DocEnum,
    DocInterface,
    DocAlias,

    DocField,
    DocType,
    DocParam,
    DocReturn,
    DocGeneric,
    DocSee,
    DocDeprecated,
    DocCast,
    DocOverload,
    DocAsync,
    DocVisibility,
    DocOther,
    DocDiagnostic,

    // Type
    TypeArray,
    TypeUnion,
    TypeFun,
    TypeGeneric,
    TypeTuple,
    TypeTable,
    TypeParen,
    TypeLiteral,
    TypeName,

    // parameter
    TypedParameter,
    // a: number
    TypedField,
    // docOther
    DocGenericDeclareList
}
