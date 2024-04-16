using EmmyLua.Config.Gen;
using NJsonSchema;
using NJsonSchema.CodeGeneration.CSharp;

var schemaJsonPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Schema", "schema.json");
var jsonSchemaString = File.ReadAllText(schemaJsonPath);
var jsonSchema = await JsonSchema.FromJsonAsync(jsonSchemaString);
var generator = new CSharpGenerator(jsonSchema);
generator.Settings.Namespace = "EmmyLua.Configuration";
generator.Settings.GenerateOptionalPropertiesAsNullable = true;
generator.Settings.EnumNameGenerator = new EnumNameGenerator();

var basePath = AppDomain.CurrentDomain.BaseDirectory;
var rootIndex = basePath.LastIndexOf("EmmyLua.Config.Gen", StringComparison.Ordinal);
var filePath = Path.Combine(basePath[..rootIndex], "EmmyLua", "Configuration", "ConfigSchema.cs");
var fileText = generator.GenerateFile();
await File.WriteAllTextAsync(filePath, fileText);
