namespace Midora.Syntax.Expressions;

public record FieldExpression : IExpression
{
    public required string FieldName { get; init; }

    public required string DeclrationType { get; init; }

    public required IExpression ThisExpression { get; init; }

    public bool IsValueType { get; init; }

    public bool IsByReference { get; init; }

    public string Emit()
    {
        if (IsValueType)
        {
            if (IsByReference)
                return $"({ThisExpression.Emit()})->{FieldName}";
            return $"{ThisExpression.Emit()}.{FieldName}";
        }

        return $"(({DeclrationType}*){ThisExpression.Emit()})->{FieldName}";
    }
}
