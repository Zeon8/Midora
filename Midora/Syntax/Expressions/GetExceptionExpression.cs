namespace Midora.Syntax.Expressions;

public class GetExceptionExpression : IExpression
{
    public string Emit() => "midora_get_exception()";
}
