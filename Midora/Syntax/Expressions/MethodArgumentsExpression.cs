using Midora.Syntax.Expressions;

namespace Midora.Syntax.Expressions;

public class MethodArgumentsExpression : IExpression
{
    public IExpression? ThisExpression { get; }

    public IEnumerable<IExpression> Arguments { get; }

    public MethodArgumentsExpression(IEnumerable<IExpression> arguments)
    {
        Arguments = arguments;
    }

    public MethodArgumentsExpression(IExpression? thisExpression, IEnumerable<IExpression> arguments)
    {
        ThisExpression = thisExpression;
        Arguments = arguments;
    }

    public string Emit()
    {
        string args = string.Join(", ", Arguments.Select(a => a.Emit()));

        if (ThisExpression is null)
            return args;

        if (Arguments.Any())
            return $"{ThisExpression.Emit()}, {args}";

        return ThisExpression.Emit();
    }
}
