using Midora.Syntax.Expressions;
using Midora.Syntax.Instructions;
using System.CodeDom.Compiler;

namespace Midora.Syntax.Instructions;

public class ReturnInstruction : IInstruction
{
    public IExpression? ReturnValue { get; }

    public ReturnInstruction() { }

    public ReturnInstruction(IExpression? returnValue)
    {
        ReturnValue = returnValue;
    }

    public void Write(IndentedTextWriter writer)
    {
        if (ReturnValue is null)
            writer.WriteLine("return;");
        else
            writer.WriteLine($"return {ReturnValue.Emit()};");
    }
}
