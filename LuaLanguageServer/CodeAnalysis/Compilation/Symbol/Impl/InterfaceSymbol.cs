using LuaLanguageServer.CodeAnalysis.Compilation.Infer;
using LuaLanguageServer.CodeAnalysis.Compilation.StubIndex;

namespace LuaLanguageServer.CodeAnalysis.Compilation.Symbol.Impl;

public class InterfaceSymbol : ClassSymbol
{
     public InterfaceSymbol(string name, SearchContext context) : base(name, context, SymbolKind.Interface)
     {
     }

     public override string Name => _name;

     // protected override void LazyInit()
     // {
     //      var classSyntax = (_context.Compilation.StubIndexImpl.ShortNameIndex.Get(_name)
     //            .FirstOrDefault(it => it is LuaShortName.Interface) as LuaShortName.Interface)?.InterfaceSyntax;
     //      if (classSyntax is not null)
     //      {
     //            _supers = classSyntax.ExtendTypeList.Select(it => _context.Infer(it)).ToList();
}
