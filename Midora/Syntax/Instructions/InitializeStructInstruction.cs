using Midora.Syntax.Expressions;
using System.CodeDom.Compiler;

namespace Midora.Syntax.Instructions;

public record InitializeStructInstruction(string Type, IExpression AddressExpression) : IInstruction
{
    public void Write(IndentedTextWriter writer)
    {
        writer.WriteLine($"*({AddressExpression.Emit()}) = ({Type}){{0}};");
    }
}
