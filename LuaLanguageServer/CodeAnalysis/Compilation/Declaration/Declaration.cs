using LuaLanguageServer.CodeAnalysis.Syntax.Node;

namespace LuaLanguageServer.CodeAnalysis.Compilation.Declaration;

[Flags]
public enum DeclarationFlag : ushort
{
    Local = 0x0001,
    Function = 0x0002,
    ClassMember = 0x0004,
    Global = 0x0008,
}

public class Declaration : DeclarationNode
{
    public string Name { get; }

    public LuaSyntaxElement SyntaxElement { get; }

    private DeclarationFlag _flag;

    private Dictionary<string, Declaration> _fields = new();

    public bool IsLocal => (_flag & DeclarationFlag.Local) != 0;

    public bool IsFunction => (_flag & DeclarationFlag.Function) != 0;

    public bool IsClassMember => (_flag & DeclarationFlag.ClassMember) != 0;

    public bool IsGlobal => (_flag & DeclarationFlag.Global) != 0;

    public Declaration(string name, int position, LuaSyntaxElement syntaxElement, DeclarationFlag flag)
        : base(position, null)
    {
        Name = name;
        SyntaxElement = syntaxElement;
        _flag = flag;
    }

    public Declaration? FindField(string name)
    {
        return _fields.TryGetValue(name, out var child) ? child : null;
    }

    public void AddField(Declaration child)
    {
        _fields.Add(child.Name, child);
    }
}
