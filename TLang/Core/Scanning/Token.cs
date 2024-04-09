namespace TLang.Core.Scanning;

public class Token(TokenType type, string lexeme, object literal, int line) {
    public readonly TokenType Type = type;
    public readonly string Lexeme = lexeme;
    public readonly object Literal = literal;
    public readonly int Line = line;

    public override string ToString() {
        return $"{Type} ${Lexeme} {Literal}";
    }
}
