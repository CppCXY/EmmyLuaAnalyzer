using EmmyLua.Configuration;
using Newtonsoft.Json.Schema.Generation;


namespace Util.Gen.Generator;

public class SchemaGenerator : IGenerator
{
    public void Generate(string projectRoot)
    {
        var generator = new JSchemaGenerator();
        generator.GenerationProviders.Add(new StringEnumGenerationProvider());
        var schema = generator.Generate(typeof(Setting));
        var filePath = Path.Combine(projectRoot, "EmmyLua/Resources", "schema.json");
        File.WriteAllText(filePath, schema.ToString());
    }
}