using System.CodeDom.Compiler;

namespace Midora.Syntax.Instructions;

public record LeaveInstruction(string Label, bool IsFinallyBlock) : IInstruction
{
    public void Write(IndentedTextWriter writer)
    {
        if (IsFinallyBlock)
            writer.WriteLine($"{Naming.ExceptionHandledVariable} = true;");
        else
            writer.WriteLine("midora_exception_frame_pop();");

        writer.WriteLine($"goto {Label};");
    }
}
