using Midora.Syntax.Expressions;
using System.CodeDom.Compiler;

namespace Midora.Syntax.Instructions;

public class NewObjectInstruction : IInstruction
{
    public required string TypeName { get; init; }

    public required string TempVaribleName { get; init; }

    public bool VariableExist { get; init; }

    public required CallStaticMethodExpression ConstructorCall { get; init; }

    public void Write(IndentedTextWriter writer)
    {
        writer.WriteLine($"{Naming.RuntimeObjectName}* {TempVaribleName} = midora_new(&{Naming.GetTypeInfoName(TypeName)});");
        writer.WriteLine(ConstructorCall.Emit() + ';');
    }
}
