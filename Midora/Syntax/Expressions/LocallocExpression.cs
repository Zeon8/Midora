namespace Midora.Syntax.Expressions;

public record LocallocExpression(IExpression Size) : IExpression
{
    public string Emit() => $"midora_localloc({Size});";
}
