using Midora.Syntax.Expressions;

namespace Midora.Syntax.Expressions;

public record ArgumentExpression(string Name) : IExpression
{
    public string Emit() => Name;
}
