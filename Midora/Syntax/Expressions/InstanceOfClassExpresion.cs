using Midora.Syntax.Expressions;

namespace Midora.Syntax.Expressions;

public record IsInstanceExpresion(IExpression Object, int TypeId) : IExpression
{
    public string Emit() => $"midora_is_instance({Object.Emit()}, {TypeId})";
}