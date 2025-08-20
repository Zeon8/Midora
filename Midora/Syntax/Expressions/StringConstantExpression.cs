namespace Midora.Syntax.Expressions;

public record StringConstantExpression(string Name) : IExpression
{
    public string Emit() => $"(RuntimeObject*)&{Name}";
}
