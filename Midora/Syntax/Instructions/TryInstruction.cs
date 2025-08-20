using System.CodeDom.Compiler;
using System.Linq.Expressions;

namespace Midora.Syntax.Instructions;

public record CatchBlockStart(string ExceptionType) : IInstruction
{
    public void Write(IndentedTextWriter writer)
    {
        writer.WriteLine($"if(midora_is_instance(midora_get_exception(), {Naming.GetTypeInfoName(ExceptionType)}()->id) != NULL)");
        writer.WriteLine('{');
        writer.Indent++;
    }
}

public class TryBlockStartInstruction(string FrameName) : IInstruction
{
    public void Write(IndentedTextWriter writer)
    {
        writer.WriteLine($"ExceptionFrame {FrameName};");
        writer.WriteLine($"midora_exception_frame_push(&{FrameName});");
        writer.WriteLine($"if(setjmp({FrameName}.buffer) == 0)");
        writer.WriteLine('{');
        writer.Indent++;
    }
}

public class BlockEndInstruction : IInstruction
{
    public void Write(IndentedTextWriter writer)
    {
        writer.Indent--;
        writer.WriteLine("}");
    }
}

public record FilterStart(string Variable) : IInstruction
{
    public void Write(IndentedTextWriter writer)
    {
        writer.WriteLine($"if({Variable} == 1)");
        writer.WriteLine('{');
        writer.Indent++;
    }
}