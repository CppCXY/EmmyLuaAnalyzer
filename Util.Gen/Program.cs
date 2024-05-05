using Util.Gen.Generator;

List<IGenerator> generators = [
    new SchemaGenerator()
];

var basePath = AppDomain.CurrentDomain.BaseDirectory;
var index = basePath.IndexOf("Util.Gen", StringComparison.Ordinal);
var path = basePath[..index];

foreach (var generator in generators)
{
    generator.Generate(path);
}
