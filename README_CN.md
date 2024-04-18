# EmmyLuaAnalyzer

## 介绍

EmmyLuaAnalyzer 项目是使用C#实现的 Lua 语言的静态分析器和语言服务，他主要分为两个部分:
1. 支持EmmyLua Doc的 Lua 代码分析核心, 他是一个独立的 Lua 代码分析库，可以用于分析 Lua 代码，生成抽象语法树，提供语义分析等功能
2. EmmyLua语言服务, 他是基于上面的代码分析核心实现的Lua 语言服务器, 提供了主要的语言服务功能，包括代码提示、代码补全、重构等功能

## 特性

- 支持主流的Lua版本，包括 Lua 5.1、Lua 5.2、Lua 5.3、Lua 5.4, LuaJIT
- 支持 EmmyLua Doc 注释并且与 Lua-Language-Server(简称为LuaLs)的Doc格式兼容
- 支持全部的语言服务特性(尚未全部支持, 目前支持主要会用到的)
- 暂不支持格式化(EmmyLuaCodeStyle也是我写的, 它本身有独立的语言服务)

## 文档

- TODO

## 使用

### 通过nuget使用EmmyLuaAnalyzer的分析核心
TODO

### 通过VSCode插件使用EmmyLuaAnalyzer的语言服务
当前可以通过在VSCode-EmmyLua中勾选`Use EmmyLuaAnalyzer`来使用EmmyLuaAnalyzer的语言服务

## 测试

当前处于测试版，欢迎大家测试并提出建议, 有很多工作还在进行中，还需要完善

## LICENSE

[MIT](./LICENSE)
