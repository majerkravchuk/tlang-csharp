namespace TLang;

public class Report {
    public static void Error(int line, string message) {
        Write(line, String.Empty, message);
    }

    public static void Write(int line, string where, string message) {
        Console.Error.WriteLine($"[line {line} Error {where}: {message}");
    }
}
