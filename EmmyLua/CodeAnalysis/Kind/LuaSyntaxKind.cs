﻿namespace EmmyLua.CodeAnalysis.Kind;

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
    TypeFun,
    TypeGeneric,
    TypeTuple,
    TypeTable,
    TypeParen,
    TypeLiteral,
    TypeName,

    // doc parameter
    TypedParameter,
    GenericParameter,
    GenericDeclareList,

    // start with '#' or '@'
    Description
}
