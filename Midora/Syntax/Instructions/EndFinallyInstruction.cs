using System.CodeDom.Compiler;

namespace Midora.Syntax.Instructions;

internal class EndFinallyInstruction : IInstruction
{
    public void Write(IndentedTextWriter writer)
    {
        writer.WriteLine($"if(!{Naming.ExceptionHandledVariable})");
        writer.Indent++;
        writer.WriteLine("midora_rethrow();");
        writer.Indent--;
    }
}
