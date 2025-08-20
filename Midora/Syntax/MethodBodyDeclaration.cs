using Midora.Syntax.Instructions;
using System.CodeDom.Compiler;

namespace Midora.Syntax;

public class MethodBodyDeclaration
{
    public required IEnumerable<IInstruction> Instructions { get; init; }

    public required IEnumerable<VariableDeclaration> Variables { get; init; }

    public required StackFrame StackFrame { get; init; }

    public void Write(IndentedTextWriter writer)
    {
        foreach (var variable in Variables)
            variable.Write(writer);

        StackFrame.Write(writer);

        foreach (var instruction in Instructions)
            instruction.Write(writer);

        StackFrame.WritePop(writer);
    }
}
