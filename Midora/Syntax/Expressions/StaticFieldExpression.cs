namespace Midora.Syntax.Expressions;

public record StaticFieldExpression(string DeclaringType, string Name) : IExpression
{
    public string Emit() => DeclaringType + '_' + Name;
}
