namespace Midora.Syntax.Expressions;

public class GetVirtualMethod : IExpression
{
    public required string VTableName { get; init; }

    public required IExpression ObjectExpression { get; init; }

    public required string MethodName { get; init; }

    public string Emit()
    {
        return $"(({VTableName}*)midora_get_vtable({ObjectExpression}))->{MethodName}";
    }
}
