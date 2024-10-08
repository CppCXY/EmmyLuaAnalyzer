namespace SyntaxNodes.Gen.Def;

public class Def(string name)
{
    public string Name { get; } = name;
    
    public List<DefField> Fields { get; } = new();
    
    public List<DefInterface> Interfaces { get; } = new();
    
    public static Def New(string name)
    {
        return new Def(name);
    }
    
    public Def Field(string name, string type)
    {
        Fields.Add(new DefField(name, type));
        return this;
    }
    
    public Def Interface(string name)
    {
        Interfaces.Add(new DefInterface(name));
        return this;
    }
}

public record struct DefField(string Name, string Type);

public record struct DefInterface(string Name);

public abstract class DefBuilder(string pathToDump, string usingLists)
{
    protected List<Def> Defs { get; } = new();

    public abstract void Init();

    public void Build(string basePath)
    {
        
    }
    
    public Def CDef(string name)
    {
        var def = Def.New(name);
        Defs.Add(def);
        return def;
    }
}