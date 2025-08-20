using Midora.Syntax.Expressions;

namespace Midora.Syntax.Expressions;

public record UnaryExpression(IExpression Value, Operator Operator) : IExpression
{
    public string Emit() => $"{Operator.Value} {Operator.Value}";
}