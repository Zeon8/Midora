namespace Midora.Syntax.Expressions;

public record GetMethodPointerInstruction(string MethodName) : IExpression
{
    public string Emit() => $"&{MethodName}";
}
