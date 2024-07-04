namespace EmmyLua.CodeAnalysis.Syntax.Kind;

public enum LuaTokenKind : ushort
{
    None,
    // KeyWord
    TkAnd,
    TkBreak,
    TkDo,
    TkElse,
    TkElseIf,
    TkEnd,
    TkFalse,
    TkFor,
    TkFunction,
    TkGoto,
    TkIf,
    TkIn,
    TkLocal,
    TkNil,
    TkNot,
    TkOr,
    TkRepeat,
    TkReturn,
    TkThen,
    TkTrue,
    TkUntil,
    TkWhile,


    TkWhitespace, // whitespace
    TkEndOfLine, // end of line
    TkPlus, // +
    TkMinus, // -
    TkMul, // *
    TkDiv, // /
    TkIDiv, // //
    TkDot, // .
    TkConcat, // ..
    TkDots, // ...
    TkComma, // ,
    TkAssign, // =
    TkEq, // ==
    TkGe, // >=
    TkLe, // <=
    TkNe, // ~=
    TkShl, // <<
    TkShr, // >>
    TkLt, // <
    TkGt, // >
    TkMod, // %
    TkPow, // ^
    TkLen, // #
    TkBitAnd, // &
    TkBitOr, // |
    TkBitXor, // ~
    TkColon, // :
    TkDbColon, // ::
    TkSemicolon, // ;
    TkLeftBracket, // [
    TkRightBracket, // ]
    TkLeftParen, // (
    TkRightParen, // )
    TkLeftBrace, // {
    TkRightBrace, // }
    TkComplex, // complex
    TkInt, // int
    TkFloat, // float

    TkName, // name
    TkString, // string
    TkLongString, // long string
    TkShortComment, // short comment
    TkLongComment, // long comment
    TkShebang, // shebang
    TkEof, // eof

    TkUnknown, // unknown

    // doc
    TkNormalStart, // -- or ---
    TkLongCommentStart, // --[[
    TkDocLongStart, // --[[@
    TkDocStart, // ---@
    TkDocTrivia, // other can not parsed
    TkDocEnumField, // ---|


    // tag
    TkTagClass, // class
    TkTagEnum, // enum
    TkTagInterface, // interface
    TkTagAlias, // alias
    TkTagModule, // module

    TkTagField, // field
    TkTagType, // type
    TkTagParam, // param
    TkTagReturn, // return
    TkTagOverload, // overload
    TkTagGeneric, // generic
    TkTagSee, // see
    TkTagDeprecated, // deprecated
    TkTagAsync, // async
    TkTagCast, // cast
    TkTagOther, // other
    TkTagVisibility, // public private protected package
    TkTagDiagnostic, // diagnostic
    TkTagMeta, // meta
    TkTagVersion, // version
    TkTagAs, // as
    TkTagNodiscard, // nodiscard
    TkTagOperator, // operator
    TkTagMapping, // mapping

    TkDocOr, // |
    TkDocContinue, // ---
    TkDocDetail, // a description
    TkNullable, // '?'
    TkDocVisibility, // public private protected package
    TkAt, // '@', invalid lua token, but for postfix completion
    TkVersionNumber, // version number
    TkTypeTemplate, // type template
    TkDocMatch, // =
    TkDocBoolean, // true false
}
