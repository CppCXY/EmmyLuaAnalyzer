namespace EmmyLua.CodeAnalysis.Kind;

public enum LuaSyntaxKind : ushort
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
    ParenExpr,
    LiteralExpr,
    ClosureExpr,
    UnaryExpr,
    BinaryExpr,
    TableExpr,
    CallExpr,
    IndexExpr,
    NameExpr,

    LocalName,
    ParamName,
    ParamList,
    CallArgList,
    TableFieldAssign,
    TableFieldValue,
    Attribute,

    // comment
    Comment,

    // doc tag
    DocClass,
    DocEnum,
    DocInterface,
    DocAlias,
    DocField,
    DocEnumField,

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
    DocMeta,
    DocOther,
    DocDiagnostic,
    DocVersion,
    DocAs,
    DocNodiscard,
    DocOperator,
    DocModule,

    DocDetailField,
    DocBody,

    // doc Type
    TypeArray,
    TypeUnion,
    TypeAggregate,
    TypeFun,
    TypeGeneric,
    TypeTuple,
    TypeTable,
    TypeParen,
    TypeLiteral,
    TypeName,
    TypeVariadic,
    TypeExpand,

    // doc parameter
    TypedParameter,
    GenericParameter,
    GenericDeclareList,
    DiagnosticNameList,
    DocAttribute,
    // start with '#' or '@'
    Description
}
