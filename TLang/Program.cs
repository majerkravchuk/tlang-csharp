using System.Globalization;
using TLang.Core.Error;
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
        var interpreter = new Interpreter(report);
        var statements = parser.Parse();

        interpreter.Interpret(statements);
    }
}
