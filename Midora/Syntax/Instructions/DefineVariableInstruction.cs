using Midora.Syntax.Expressions;
using Midora.Syntax.Instructions;
using System.CodeDom.Compiler;

namespace Midora.Syntax.Instructions;

public record DefineVariableInstruction : IInstruction
{
    public required string Type { get; init; }

    public required string Name { get; init; }

    public IExpression? DefaultValue { get; init; }

    public void Write(IndentedTextWriter writer)
    {
        if (DefaultValue is null)
            writer.WriteLine($"{Type} {Name};");
        else
            writer.WriteLine($"{Type} {Name} = {DefaultValue.Emit()};");
    }
}
