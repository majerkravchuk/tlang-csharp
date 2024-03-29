namespace TTool;

class GenerateAst {
    static void Main(string[] args) {
        if (args.Length != 1) {
            Console.Error.WriteLine("Usage: generate_ast <output directory>");
            Environment.Exit(64);
        }

        string outputDir = args[0];

        DefineAst(outputDir, "Expr", new Dictionary<string, string> {
            { "Binary", "Expr Left, Token Opt, Expr Right" },
            { "Grouping", "Expr Expression" },
            { "Literal", "Object Value" },
            { "Unary", "Token Opt, Expr Right" },
        });
    }

    private static void DefineAst(string outputDir, string baseName, Dictionary<string, string> types) {
        foreach (var type in types) {
            DefineType(outputDir, baseName, type.Key, type.Value);
        }
    }

    private static void DefineType(string outputDir, string baseName, string className, string fields) {
        var path = outputDir + "/" + className + ".cs";
        var writer = new StreamWriter(File.Open(path, FileMode.OpenOrCreate));

        writer.WriteLine("namespace TLang.Ast;");
        writer.WriteLine();
        writer.WriteLine($"public class {className} : {baseName} {{");

        var fieldsArray = fields.Split(", ");

        foreach (var field in fieldsArray) {
            writer.WriteLine($"    public {field};");
        }
        writer.WriteLine();

        var initializers = fieldsArray.Select(f => {
            var parts = f.Split(" ");
            var argName = char.ToLowerInvariant(Convert.ToChar(parts[1][0])) + parts[1][1..];
            return ($"{parts[0]} {argName}", $"{parts[1]} = {argName}");
        }).ToList();

        writer.WriteLine($"    public {className} ({string.Join(", ", initializers.Select(field => field.Item1))}) {{");
        foreach (var field in initializers) {
            writer.WriteLine($"        {field.Item2};");
        }
        writer.WriteLine("    }");
        writer.WriteLine("}");
        writer.WriteLine();

        writer.Flush();
        writer.Close();
    }
}
