namespace Midora.Syntax.Expressions;

public class UnboxExpression : IExpression
{
    public required string TypeName { get; init; }

    public required IExpression BoxExpression { get; init; }

    public string Emit() => $"*(({TypeName}*)midora_box_get_value({BoxExpression.Emit()}))";
}
