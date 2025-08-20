using Midora.Syntax.Expressions;
using System.CodeDom.Compiler;

namespace Midora.Syntax.Instructions;

public record EndFilterInstruction(string VariableName, IExpression ValueExpression) : IInstruction
{
    public void Write(IndentedTextWriter writer)
    {
        writer.WriteLine($"{VariableName} = {ValueExpression.Emit()};");
    }
}
