using LuaLanguageServer.CodeAnalysis.Compilation.Analyzer.Declaration;
using LuaLanguageServer.CodeAnalysis.Compilation.Analyzer.Infer;
using LuaLanguageServer.CodeAnalysis.Compilation.Symbol;
using LuaLanguageServer.CodeAnalysis.Syntax.Node;
using LuaLanguageServer.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace LuaLanguageServer.CodeAnalysis.Compilation.Type;

public class LuaLazyType(LuaSyntaxElement? typeElement, int retId = 0) : LuaType(TypeKind.LazyType)
{
    private ILuaType? _reaLuaType;

    public override IEnumerable<Declaration> GetMembers(SearchContext context)
    {
        return GetRealType(context).GetMembers(context);
    }

    public ILuaType GetRealType(SearchContext context)
    {
        _reaLuaType ??= context.Infer(typeElement);
        if (_reaLuaType is LuaMultiRetType multi)
        {
            return multi.GetRetType(retId) ?? context.Compilation.Builtin.Unknown;
        }

        return _reaLuaType;
    }
}

public class LuaLazyIterType(List<LuaExprSyntax> exprList, int itPosition) : LuaType(TypeKind.LazyType)
{
    private ILuaType? _reaLuaType;

    private List<LuaExprSyntax> _exprList = exprList;

    private int _itPosition = itPosition;

    public override IEnumerable<Declaration> GetMembers(SearchContext context)
    {
        return GetRealType(context).GetMembers(context);
    }

    public ILuaType GetRealType(SearchContext context)
    {
        // _reaLuaType ??= context.Infer(_typeElement);
        // if (_reaLuaType is LuaMultiRetType multi)
        // {
        //     return multi.GetRetType(_retId) ?? context.Compilation.Builtin.Unknown;
        // }

        return _reaLuaType;
    }
}
