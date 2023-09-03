namespace LuaLanguageServer.LuaCore.Kind;

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
    TkNumber, // number
    TkComplex, // complex
    TkInt, // int
    TkName, // name
    TkString, // string
    TkLongString, // long string
    TkShortComment, // short comment
    TkLongComment, // long comment
    TkShebang, // shebang
    TkEof, // eof

    // error
    TkUnknown, // unknown
    TkUnCompleteLongStringStart, // [==
    TkUnFinishedLongString, // ]]
    TkUnFinishedString, // string

    // doc
    TkNormalStart, // --
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

    TkDocOr, // |
    TkDocContinue, // ---
    TkDocDescription, // '#' or '@' description
    TkNullable, // '?'
    TkDocVisibility // public private protected package
}
