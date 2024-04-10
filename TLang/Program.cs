using TLang.Core;
using TLang.Core.Interpretation;
using TLang.Core.Parsing;
using TLang.Core.Scanning;

namespace TLang;

class Program {
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
        var report = new Report();
        var interpreter = new Interpreter(report);

        Run(File.ReadAllText(path), interpreter, report);
        if (_hadError) Environment.Exit(65);
    }

    private static void RunPrompt() {
        var report = new Report();
        var interpreter = new Interpreter(report);

        while (true) {
            Console.Write("> ");
            var line = Console.ReadLine();

            if (line == null)
                break;

            Run(line, interpreter, report);
            _hadError = false;
        }
    }

    private static void Run(string source, Interpreter interpreter, IReport report) {
        var scanner = new Scanner(source, report);
        var tokens = scanner.ScanTokens();

        var parser = new Parser(tokens, report);
        var statements = parser.Parse();

        interpreter.Interpret(statements);
    }
}
