using Midora.Syntax.Expressions;
using Midora.Syntax.Instructions;
using System.CodeDom.Compiler;

namespace Midora.Syntax.Instructions;

public record GotoIfInstruction(IExpression Condition, string Label) : IInstruction
{
    public void Write(IndentedTextWriter writer)
    {
        writer.WriteLine($"if({Condition.Emit()})");
        writer.Indent++;
        writer.WriteLine($"goto {Label};");
        writer.Indent--;
    }
}
