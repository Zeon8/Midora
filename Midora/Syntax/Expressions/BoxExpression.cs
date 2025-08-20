namespace Midora.Syntax.Expressions;

public record BoxExpression : IExpression
{
    public required string TypeName { get; init; }
    
    public required IExpression ValueExpression { get; init; }

    public string Emit()
    {
        return $"midora_box(&{ValueExpression.Emit()}, ${Naming.GetTypeInfoName(TypeName)})";
    }
}
