using System.Globalization;
using EmmyLua.CodeAnalysis.Document.Version;
using EmmyLua.CodeAnalysis.Kind;
using EmmyLua.CodeAnalysis.Syntax.Tree;


namespace EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;

public class LuaStringToken(int index, LuaSyntaxTree tree)
    : LuaSyntaxToken(index, tree)
{
    public string Value => Tree.GetStringTokenValue(ElementId);

    public override string ToString()
    {
        return Value;
    }
}

public class LuaNumberToken(int index, LuaSyntaxTree tree) : LuaSyntaxToken(index, tree)
{
    public bool IsInteger => Kind == LuaTokenKind.TkInt;

    public bool IsComplex => Kind == LuaTokenKind.TkComplex;

    public bool IsFloat => Kind == LuaTokenKind.TkFloat;

    public override string ToString()
    {
        return Text.ToString();
    }
}

public class LuaIntegerToken(int index, LuaSyntaxTree tree) : LuaNumberToken(index, tree)
{
    public long Value => Tree.GetIntegerTokenValue(ElementId).Item1;

    public string Suffix => Tree.GetIntegerTokenValue(ElementId).Item2;

    public override string ToString()
    {
        return $"{Value}{Suffix}";
    }
}

public class LuaFloatToken(int index, LuaSyntaxTree tree) : LuaNumberToken(index, tree)
{
    public double Value => Tree.GetNumberTokenValue(ElementId);

    public override string ToString()
    {
        return Value.ToString(CultureInfo.InvariantCulture);
    }
}

public class LuaComplexToken(int index, LuaSyntaxTree tree) : LuaNumberToken(index, tree)
{
    public string Value => Text.ToString();

    public override string ToString()
    {
        return Value;
    }
}

public class LuaNilToken(int index, LuaSyntaxTree tree) : LuaSyntaxToken(index, tree);

public class LuaBoolToken(int index, LuaSyntaxTree tree) : LuaSyntaxToken(index, tree)
{
    public bool Value => Text == "true";
}

public class LuaDotsToken(int index, LuaSyntaxTree tree) : LuaSyntaxToken(index, tree);

public class LuaNameToken(int index, LuaSyntaxTree tree) : LuaSyntaxToken(index, tree);

public class LuaWhitespaceToken(int index, LuaSyntaxTree tree) : LuaSyntaxToken(index, tree);

public class LuaVersionNumberToken(int index, LuaSyntaxTree tree) : LuaSyntaxToken(index, tree)
{
    public VersionNumber Version => Tree.GetVersionNumber(ElementId);
}

public class LuaTemplateTypeToken(int index, LuaSyntaxTree tree) : LuaSyntaxToken(index, tree)
{
    public string Name => Tree.GetStringTokenValue(ElementId);
}
