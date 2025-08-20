namespace Midora.Syntax.Expressions;

public class GetInterfaceMethod : IExpression
{
    public required string VTableName { get; init; }

    public required IExpression ObjectExpression { get; init; }

    public required string InterfaceType { get; init; }

    public required string MethodName { get; init; }

    public string Emit()
    {
        return $"(({VTableName}*)midora_resolve_interface_vtable({ObjectExpression.Emit()}, &{Naming.GetTypeInfoName(InterfaceType)}))->{MethodName}";
    }
}
