namespace EmmyLua.CodeAnalysis.Syntax.Kind;

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
    DocMapping,

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
    TypeTemplate,
    TypeMatch,

    // doc parameter
    TypedParameter,
    GenericParameter,
    GenericDeclareList,
    DiagnosticNameList,
    DocAttribute,
    // start with '#' or '@'
    Description,

    // [<|>] [<framework>] <version>, <version> can be '5.1', '5.2', '5.3', '5.4', 'JIT', <framework> can be 'openresty'
    Version,
}
