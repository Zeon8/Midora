using Midora.Syntax.Instructions;
using System.CodeDom.Compiler;

namespace Midora.Syntax.Instructions;

public class BreakInstruction : IInstruction
{
    public void Write(IndentedTextWriter writer)
    {
        writer.Write("break;");
    }
}
