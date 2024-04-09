using TLang.Core.Error;
using TLang.Core.Scanning;

namespace TLang.Core.Parsing;

public class RuntimeEnvironment {
    private readonly Dictionary<string, object?> _values = new();
    private RuntimeEnvironment? _enclosing;

    public RuntimeEnvironment() {
        _enclosing = null;
    }

    public RuntimeEnvironment(RuntimeEnvironment? enclosing) {
        _enclosing = enclosing;
    }

    public object? Get(Token name) {
        if (_values.TryGetValue(name.Lexeme, out var value)) {
            return value;
        }

        if (_enclosing != null) return _enclosing.Get(name);

        throw new RuntimeError(name, "Undefined variable '" + name.Lexeme + "'.");
    }

    public void Assign(Token name, object? value) {
        if (_values.ContainsKey(name.Lexeme)) {
            _values[name.Lexeme] = value;
            return;
        }

        if (_enclosing != null) {
            _enclosing.Assign(name, value);
            return;
        }

        throw new RuntimeError(name, "Undefined variable '" + name.Lexeme + "'.");
    }

    public void Define(string name, object? value) {
        _values[name] = value;
    }
}
