using Midora.Syntax.Expressions;

namespace Midora.Syntax.Expressions;

public record LengthExpression(IExpression Array) : IExpression
{
    public string Emit() => $"midora_array_get_length({Array.Emit()})";
}
