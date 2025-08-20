using System.CodeDom.Compiler;

namespace Midora.Syntax.Instructions;

public interface IInstruction
{
    void Write(IndentedTextWriter writer);
}
