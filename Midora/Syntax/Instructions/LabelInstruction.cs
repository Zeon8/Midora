using Midora.Syntax.Instructions;
using System.CodeDom.Compiler;

namespace Midora.Syntax.Instructions;

public record LabelInstruction(string Label, IInstruction Instruction) : IInstruction
{
    public void Write(IndentedTextWriter writer)
    {
        writer.WriteLineNoTabs($"{Label}:");
        Instruction.Write(writer);
    }
}
