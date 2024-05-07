using System.Reflection;
using System.Runtime.Serialization;
using EmmyLua.Configuration;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using Newtonsoft.Json.Schema.Generation;


namespace Util.Gen.Generator;

public class SchemaGenerator : IGenerator
{
    public void Generate(string projectRoot)
    {
        var generator = new JSchemaGenerator();
        generator.GenerationProviders.Add(new StringEnumGenerationProvider());
        generator.GenerationProviders.Add(new EnumKeyDictionaryGenerationProvider());
        var schema = generator.Generate(typeof(Setting));
        var filePath = Path.Combine(projectRoot, "EmmyLua/Resources", "schema.json");
        File.WriteAllText(filePath, schema.ToString());
    }
}

public class EnumKeyDictionaryGenerationProvider : JSchemaGenerationProvider
{
    public override JSchema GetSchema(JSchemaTypeGenerationContext context)
    {
        if (context.ObjectType.IsGenericType &&
            context.ObjectType.GetGenericTypeDefinition() == typeof(Dictionary<,>) &&
            context.ObjectType.GetGenericArguments()[0].IsEnum &&
            context.ObjectType.GetGenericArguments()[1].IsEnum)
        {
            var keyType = context.ObjectType.GetGenericArguments()[0];
            var valueSchema = context.Generator.Generate(context.ObjectType.GetGenericArguments()[1]);

            var schema = new JSchema
            {
                Type = JSchemaType.Object,
            };
            
            foreach (var field in keyType.GetFields(BindingFlags.Public | BindingFlags.Static))
            {
                var attribute = field.GetCustomAttribute<EnumMemberAttribute>();
                if (attribute?.Value is {} key)
                {
                    schema.Properties[key] = valueSchema;
                }
            }
            
            schema.Properties["*"] = valueSchema;

            return schema;
        }

        return null;
    }

    public override bool CanGenerateSchema(JSchemaTypeGenerationContext context)
    {
        return context.ObjectType.IsGenericType &&
               context.ObjectType.GetGenericTypeDefinition() == typeof(Dictionary<,>) &&
               context.ObjectType.GetGenericArguments()[0].IsEnum &&
               context.ObjectType.GetGenericArguments()[1].IsEnum;
    }
}