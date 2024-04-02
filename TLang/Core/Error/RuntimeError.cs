using TLang.Core.Scanning;

namespace TLang.Core.Error;

public class RuntimeError(Token token, string message) : ApplicationException(message) {
    public readonly Token Token = token;
}
