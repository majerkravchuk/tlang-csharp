using System.Globalization;
using TLang.Core.Error;
using TLang.Core.Interpretation;
using TLang.Core.Parsing;
using TLang.Core.Scanning;

namespace TLang;

class Program {
    private static readonly Interpreter Interpreter = new();
    private static bool _hadError;

    static void Main(string[] args) {
        switch (args.Length) {
            case > 1:
                Console.WriteLine("Usage: tlang: [script]");
                Environment.Exit(64);
                break;
            case 1:
                RunFile(args[0]);
                break;
            default:
                RunPrompt();
                break;
        }
    }

    private static void RunFile(string path) {
        Run(File.ReadAllText(path));
        if (_hadError) Environment.Exit(65);
    }

    private static void RunPrompt() {
        while (true) {
            Console.Write("> ");
            var line = Console.ReadLine();

            if (line == null)
                break;

            Run(line);
            _hadError = false;
        }
    }

    private static void Run(string source) {
        var report = new Report();
        var scanner = new Scanner(source, report);
        var tokens = scanner.ScanTokens();

        var parser = new Parser(tokens, report);
        var expression = parser.Parse();
        try {
            var value = Interpreter.Interpret(expression);
            Console.WriteLine(Stringify(value));
        } catch (RuntimeError error) {
            report.RuntimeError(error);
            return;
        }

        Console.WriteLine(new AstPrinter().Print(expression));
    }

    private static string Stringify(object value) {
        switch (value) {
            case null:
                return "nil";
            case double dv: {
                var text = dv.ToString(CultureInfo.InvariantCulture);
                if (text.EndsWith(".0"))
                    text = text[..^2];
                return text;
            }
            default:
                return value.ToString()!;
        }
    }
}
