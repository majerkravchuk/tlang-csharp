using TLang.Core.Error;

namespace TLang.Core;

public interface IReport {
    void Write(int line, string where, string message);
    void Error(int line, string message);
    void RuntimeError(RuntimeError error);
}
