using Midora;
using Midora.Syntax.Expressions;
using Midora.Syntax.Instructions;
using System.CodeDom.Compiler;

namespace Midora.Syntax.Instructions;

public class NewArrayInstruction : IInstruction
{
    public required string ElementTypeName { get; init; }

    public required string TempVaribleName { get; init; }

    public required IExpression LengthExpression { get; init; }

    public void Write(IndentedTextWriter writer)
    {
        string length = LengthExpression.Emit();
        string type = Naming.GetTypeInfoName($"System_Array_{ElementTypeName}");
        writer.WriteLine($"{Naming.RuntimeObjectName}* {TempVaribleName} = midora_array_new(&{type}, {length});");
    }
}
