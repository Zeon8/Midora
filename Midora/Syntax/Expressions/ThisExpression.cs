using Midora;
using Midora.Syntax.Expressions;

namespace Midora.Syntax.Expressions;

public class ThisExpression : IExpression
{
    public string Emit() => Naming.ThisArgument;
}
