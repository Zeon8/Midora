namespace Midora.Syntax.Expressions;

public record GetRuntimeTypeToken(string TypeName) : IExpression
{
    public string Emit() => $"midora_get_type_handle({Naming.GetTypeInfoName(TypeName)})";
}
