using Midora.Syntax.Expressions;

namespace Midora.Syntax.Expressions;

public record CallStaticMethodExpression : IExpression
{
    public required string Name { get; init; }

    public required MethodArgumentsExpression Arguments { get; init; }

    public string Emit() => $"{Name}({Arguments.Emit()})";
}
