﻿namespace TLang;

class Program {
    private static bool hadError = false;

    static void Main(string[] args) {
        if (args.Length > 1) {
            Console.WriteLine("Usage: tlang: [script]");
            Environment.Exit(64);
        } else if (args.Length == 1) {
            RunFile(args[0]);
        } else {
            RunPrompt();
        }
    }

    private static void RunFile(String path) {
        Run(File.ReadAllText(path));
        if (hadError) Environment.Exit(65);
    }

    private static void RunPrompt() {
        while (true) {
            Console.Write("> ");
            var line = Console.ReadLine();

            if (line == null)
                break;

            Run(line);
            hadError = false;
        }
    }

    private static void Run(string source) {
        var scanner = new Scanner(source);
        var tokens = scanner.ScanTokens();

        foreach (var token in tokens) {
            Console.WriteLine(token);
        }
    }

    private static void Error(int line, string message) {
        Report(line, String.Empty, message);
    }

    private static void Report(int line, string where, string message) {
        Console.Error.WriteLine($"[line {line} Error {where}: {message}");
    }
}