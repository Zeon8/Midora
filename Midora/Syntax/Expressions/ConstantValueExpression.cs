using Midora.Syntax.Expressions;

namespace Midora.Syntax.Expressions;

public record ConstantValueExpression(string Value) : IExpression
{
    public string Emit() => Value;
}
