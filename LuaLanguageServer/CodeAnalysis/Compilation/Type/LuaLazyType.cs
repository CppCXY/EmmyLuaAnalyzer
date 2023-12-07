using LuaLanguageServer.CodeAnalysis.Compilation.Analyzer.Infer;
using LuaLanguageServer.CodeAnalysis.Compilation.Symbol;
using LuaLanguageServer.CodeAnalysis.Syntax.Node;
using LuaLanguageServer.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace LuaLanguageServer.CodeAnalysis.Compilation.Type;

public class LuaLazyType : LuaType
{
    private ILuaType? _reaLuaType;

    private LuaSyntaxElement? _typeElement;

    private int _retId;

    public LuaLazyType(LuaSyntaxElement? typeElement, int retId = 0) : base(TypeKind.LazyType)
    {
        _typeElement = typeElement;
        _retId = retId;
    }

    public override IEnumerable<ILuaSymbol> GetMembers(SearchContext context)
    {
        return GetRealType(context).GetMembers(context);
    }

    public ILuaType GetRealType(SearchContext context)
    {
        _reaLuaType ??= context.Infer(_typeElement);
        if (_reaLuaType is LuaMultiRetType multi)
        {
            return multi.GetRetType(_retId) ?? context.Compilation.Builtin.Unknown;
        }

        return _reaLuaType;
    }
}

public class LuaLazyIterType : LuaType
{
    private ILuaType? _reaLuaType;

    private List<LuaExprSyntax> _exprList;

    private int _itPosition;

    public LuaLazyIterType(List<LuaExprSyntax> exprList, int itPosition) : base(TypeKind.LazyType)
    {
        _exprList = exprList;
        _itPosition = itPosition;
    }

    public override IEnumerable<ILuaSymbol> GetMembers(SearchContext context)
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
