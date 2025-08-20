using Midora.Syntax.Expressions;
using Midora.Syntax.Instructions;
using Mono.Cecil.Cil;
using Mono.Cecil;

namespace Midora.Transpilers;

public partial class ILTranspiler
{
    private NoInstruction TranspileLdloc(VariableReference varible) => TranspileLdloc(varible.Index);

    private NoInstruction TranspileLdloc(int localIndex)
    {
        var local = _context.Variables[localIndex];
        _context.Stack.Push(new StackItem(local.Type, new VariableExpression(local.Name)));
        return s_noInstruction;
    }

    private IInstruction TranspileLdloca(VariableReference local)
    {
        var varible = _context.Variables[local.Index];

        var expression = new TakeAddressExpression(new VariableExpression(varible.Name));
        _context.Stack.Push(new StackItem(new ByReferenceType(local.VariableType), expression));
        return s_noInstruction;
    }

    private IInstruction TranspileStloc(VariableReference variable) => TranspileStloc(variable.Index);

    private IInstruction TranspileStloc(int index)
    {
        var local = _context.Variables[index];
        StackItem item = _context.Stack.Pop();

        return new AssignInstruction(new VariableExpression(local.Name), item.Expression);
    }
}
