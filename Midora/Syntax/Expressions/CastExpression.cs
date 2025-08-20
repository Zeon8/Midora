namespace Midora.Syntax.Expressions;

public class CastExpression(IExpression Expression, string Type) : IExpression
{
    public string Emit() => $"({Type}){Expression.Emit()}";
}
