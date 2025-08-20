using Midora.Syntax.Expressions;

namespace Midora.Syntax.Expressions;

public record DereferenceExpression(IExpression AddressExpression) : IExpression
{
    public string Emit() => "*" + AddressExpression.Emit();
}
