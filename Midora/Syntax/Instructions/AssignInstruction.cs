using Midora.Syntax.Expressions;
using Midora.Syntax.Instructions;
using System.CodeDom.Compiler;

namespace Midora.Syntax.Instructions;

public class AssignInstruction(IExpression LeftValue, IExpression RightValue) : IInstruction
{
    public void Write(IndentedTextWriter writer)
    {
        writer.WriteLine($"{LeftValue.Emit()} = {RightValue.Emit()};");
    }
}
