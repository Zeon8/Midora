using Midora.Syntax.Expressions;
using Midora.Syntax.Instructions;
using System.CodeDom.Compiler;

namespace Midora.Syntax.Instructions;

public record WriteExpressionInstruction(IExpression Expression) : IInstruction
{
    public void Write(IndentedTextWriter writer)
        => writer.WriteLine(Expression.Emit() + ";");
}
