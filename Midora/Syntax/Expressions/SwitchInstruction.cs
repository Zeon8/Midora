using Midora.Syntax.Expressions;
using Midora.Syntax.Instructions;
using System.CodeDom.Compiler;

namespace Midora.Syntax.Expressions;

public record SwitchInstruction(IExpression Value, IReadOnlyList<string> Labels) : IInstruction
{
    public void Write(IndentedTextWriter writer)
    {
        writer.WriteLine($"switch ({Value.Emit}){{");
        writer.Indent++;
        for (int i = 0; i < Labels.Count; i++)
        {
            var label = Labels[i];
            writer.WriteLine($"case {i}:");
            writer.Indent++;
            writer.WriteLine($"goto {label};");
            writer.WriteLine("break;");
            writer.Indent--;
        }
        writer.Indent--;
        writer.WriteLine('}');
    }
}
