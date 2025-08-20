using Midora.Syntax.Expressions;
using Mono.Cecil;

namespace Midora;

public struct StackItem
{
    public TypeReference Type { get; }

    public IExpression Expression { get; }

    public StackItem(TypeReference type, IExpression expression)
    {
        Type = type;
        Expression = expression;
    }
}
