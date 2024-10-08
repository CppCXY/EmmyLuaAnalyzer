namespace SyntaxNodes.Gen.Def;

public class LuaDefs() : DefBuilder("LuaSyntaxDefs", "")
{
    public override void Init()
    {
        CDef("LuaSourceSyntax")
            .Field("Block", "LuaBlockSyntax");

        CDef("LuaBlockSyntax")
            .Field("StatList", "List<LuaStatSyntax>");
    }
}