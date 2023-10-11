using LuaLanguageServer.CodeAnalysis.Compilation.Infer;
using LuaLanguageServer.CodeAnalysis.Compilation.StubIndex;

namespace LuaLanguageServer.CodeAnalysis.Compilation.Symbol.Impl;

public class ClassSymbol : LuaSymbol
{
    private bool _lazyInit = false;

    private SearchContext _context;

    private List<ILuaSymbol>? _supers = null;

    private List<ILuaSymbol>? _members = null;

    public ClassSymbol(string name, SearchContext context, SymbolKind kind = SymbolKind.Class) : base(name, kind)
    {
        _context = context;
    }

    protected virtual void LazyInit()
    {
        var classSyntax = (_context.Compilation.StubIndexImpl.ShortNameIndex.Get(Name)
            .FirstOrDefault(it => it is LuaShortName.Class) as LuaShortName.Class)?.ClassSyntax;
        if (classSyntax is not null)
        {
            _supers = classSyntax.ExtendTypeList.Select(it => _context.Infer(it)).ToList();
            // todo: members
            // _members = _context.Compilation.StubIndexImpl.Members.Get(classSyntax)
            //     .Where(it=> it )
            //     .Select(it => _context.Infer(it))
            //     .ToList();
        }
    }

    public IEnumerable<ILuaSymbol> Supers
    {
        get
        {
            if (!_lazyInit)
            {
                _lazyInit = true;
                LazyInit();
            }

            return _supers ?? Enumerable.Empty<ILuaSymbol>();
        }
    }

    // TODO: super members
    public override IEnumerable<ILuaSymbol> Members
    {
        get
        {
            if (!_lazyInit)
            {
                _lazyInit = true;
                LazyInit();
            }

            return _members ?? Enumerable.Empty<ILuaSymbol>();
        }
    }
}
