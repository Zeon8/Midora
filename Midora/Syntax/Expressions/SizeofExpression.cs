using Midora.Syntax.Expressions;

namespace Midora.Syntax.Expressions;

public record SizeofExpression(string Type) : IExpression
{
    public string Emit() => $"sizeof({Type})";
}
