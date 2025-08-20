using Midora.Syntax.Expressions;

namespace Midora.Syntax.Expressions;

public record BinaryExpression(IExpression LeftValue, IExpression RightValue, Operator Operator) : IExpression
{
    public string Emit() => $"{LeftValue.Emit()} {Operator.Value} {RightValue.Emit()}";
}
