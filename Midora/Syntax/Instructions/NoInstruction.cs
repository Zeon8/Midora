using Midora.Syntax.Instructions;
using System.CodeDom.Compiler;

namespace Midora.Syntax.Instructions;

public class NoInstruction : IInstruction
{
    public void Write(IndentedTextWriter writer) { }
}
