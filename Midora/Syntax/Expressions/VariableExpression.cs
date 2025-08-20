using Midora.Syntax.Expressions;

namespace Midora.Syntax.Expressions;

public class VariableExpression(string Name) : IExpression
{
    public string Emit() => Name;
}
