using Midora.Syntax.Expressions;

namespace Midora.Syntax.Expressions;

public class TakeAddressExpression(IExpression Value) : IExpression
{
    public string Emit() => $"&{Value.Emit()}";
}
