using LuaLanguageServer.CodeAnalysis.Compilation.Infer;
using LuaLanguageServer.CodeAnalysis.Compilation.StubIndex;

namespace LuaLanguageServer.CodeAnalysis.Compilation.Symbol.Impl;

public class EnumSymbol : LuaSymbol
{
    private bool _lazyInit = false;

    private SearchContext _context;

    private ILuaSymbol _base = null;

    private List<ILuaSymbol>? _members = null;

    protected string _name;

    public EnumSymbol(string name, SearchContext context) : base(SymbolKind.Enum)
    {
        _name = name;
        _context = context;
    }

    public override string Name => _name;

    protected virtual void LazyInit()
    {
        var enumSyntax = (_context.Compilation.StubIndexImpl.ShortNameIndex.Get(_name)
            .FirstOrDefault(it => it is LuaShortName.Enum) as LuaShortName.Enum)?.EnumSyntax;
        if (enumSyntax is not null)
        {
            _base = _context.Infer(enumSyntax.BaseType);
            // todo: members
            // _members = _context.Compilation.StubIndexImpl.Members.Get(classSyntax)
            //     .Where(it=> it )
            //     .Select(it => _context.Infer(it))
            //     .ToList();
        }
    }

    public ILuaSymbol? Base
    {
        get
        {
            if (!_lazyInit)
            {
                _lazyInit = true;
                LazyInit();
            }

            return _base;
        }
    }

    // TODO: members
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
