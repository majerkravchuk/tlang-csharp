using TLang.Core;
using TLang.Core.Error;

namespace TLang;

public class Report : IReport {
    public void Error(int line, string message) {
        Write(line, string.Empty, message);
    }

    public void Write(int line, string where, string message) {
        Console.Error.WriteLine($"[line {line} Error {where}: {message}");
    }

    public void RuntimeError(RuntimeError error) {
        Console.Error.WriteLine(error.Message + "\n[line " + error.Token.Line + "]");
    }
}
