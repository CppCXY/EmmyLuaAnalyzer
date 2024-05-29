using System.Globalization;
using EmmyLua.CodeAnalysis.Document.Version;
using EmmyLua.CodeAnalysis.Kind;
using EmmyLua.CodeAnalysis.Syntax.Tree;
using EmmyLua.CodeAnalysis.Syntax.Tree.Green;

namespace EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;

public class LuaStringToken(string value, int index, LuaSyntaxTree tree)
    : LuaSyntaxToken(index, tree)
{
    public string Value { get; } = value;

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

public class LuaIntegerToken(long value, string suffix, int index, LuaSyntaxTree tree) : LuaNumberToken(index, tree)
{
    public long Value { get; } = value;

    public string Suffix { get; } = suffix;

    public override string ToString()
    {
        return $"{Value}{Suffix}";
    }
}

public class LuaFloatToken(double value, int index, LuaSyntaxTree tree) : LuaNumberToken(index, tree)
{
    public double Value { get; } = value;

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

public class LuaVersionNumberToken(VersionNumber version, int index, LuaSyntaxTree tree) : LuaSyntaxToken(index, tree)
{
    public VersionNumber Version { get; } = version;
}

public class LuaTemplateTypeToken(string name, int index, LuaSyntaxTree tree) : LuaSyntaxToken(index, tree)
{
    public string Name { get; } = name;
}
