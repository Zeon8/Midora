using Midora.Syntax.Instructions;
using System.CodeDom.Compiler;

namespace Midora.Syntax.Instructions;

public record GotoInstruction(string Label) : IInstruction
{
    public void Write(IndentedTextWriter writer) => writer.WriteLine($"goto {Label};");
}
