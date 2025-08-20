using Midora.Syntax.Expressions;

namespace Midora.Syntax.Expressions;

public class NullExpression : IExpression
{
    public string Emit() => "NULL";
}
