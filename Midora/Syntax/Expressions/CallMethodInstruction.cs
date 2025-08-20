namespace Midora.Syntax.Expressions;

public class CallMethodInstruction(IExpression MethodExpression, MethodArgumentsExpression Arguments)
    : IExpression
{
    public string Emit() => $"{MethodExpression.Emit()}({Arguments.Emit()})";
}