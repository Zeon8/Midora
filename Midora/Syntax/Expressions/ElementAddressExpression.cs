using Midora.Syntax.Expressions;

namespace Midora.Syntax.Expressions;

public class ElementAddressExpression : IExpression
{
    public required IExpression Array { get; init; }

    public required IExpression Index { get; init; }

    public required string Type { get; init; }

    public string Emit()
    {
        return $"({Type}*)midora_array_get_element_ref({Array.Emit()},{Index.Emit()})";
    }
}
