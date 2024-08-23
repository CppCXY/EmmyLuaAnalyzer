﻿using EmmyLua.CodeAnalysis.Compile.Kind;

namespace EmmyLua.CodeAnalysis.Compile.Parser;

public class UnexpectedTokenException : ApplicationException
{
    // 定义一个私有字段，用于存储 Token 值
    private LuaTokenKind Token { get; set; }

    public UnexpectedTokenException()
    {
    }

    public UnexpectedTokenException(string message) : base(message)
    {
    }

    public UnexpectedTokenException(string message, Exception inner) : base(message, inner)
    {
    }

    public UnexpectedTokenException(string message, LuaTokenKind token) : this(message)
    {
        Token = token;
    }
}
