# EmmyLuaAnalyzer

## Introduction

The EmmyLuaAnalyzer project is a static analyzer and language service for the Lua language implemented in C#. It mainly consists of two parts:
1. Lua code analysis core that supports EmmyLua Doc. It is an independent Lua code analysis library that can be used to analyze Lua code, generate abstract syntax trees, provide semantic analysis, and other functions.
2. EmmyLua language service, which is a Lua language server based on the above code analysis core. It provides the main language service functions, including code hints, code completion, refactoring, and other features.

## Features

- Supports mainstream Lua versions, including Lua 5.1, Lua 5.2, Lua 5.3, Lua 5.4, LuaJIT.
- Supports EmmyLua Doc comments and is compatible with the Doc format of Lua-Language-Server (abbreviated as LuaLs).
- Supports all language service features (not all are supported yet, currently supports the main ones that will be used).
- Formatting is not supported yet (EmmyLuaCodeStyle, which I also wrote, has its own independent language service).

## Documentation

[.emmyrc.json config](./docs/.emmyrc.json_EN.md)

## Usage

### Use EmmyLuaAnalyzer's analysis core through nuget
TODO

### Use EmmyLuaAnalyzer's language server through the VSCode plugin
You can currently use EmmyLuaAnalyzer's language server by install `VSCode-EmmyLua`.

## LICENSE

[MIT](./LICENSE)
