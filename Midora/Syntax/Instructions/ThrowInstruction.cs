using Midora.Syntax.Expressions;
using System.CodeDom.Compiler;

namespace Midora.Syntax.Instructions;

public class ThrowInstruction(IExpression ObjectExpression) : IInstruction
{
    public void Write(IndentedTextWriter writer)
    {
        writer.WriteLine($"midora_throw({ObjectExpression.Emit()});");
    }
}

public class RethrowInstruction : IInstruction
{
    public void Write(IndentedTextWriter writer) => writer.WriteLine($"midora_rethrow();");
}

